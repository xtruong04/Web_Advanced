using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using QRCoder; // Đừng quên cài Nuget: QRCoder
using System.Drawing;
using System.Drawing.Imaging;

namespace ClothesShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public CheckoutController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = CartHelper.GetCart(HttpContext.Session) ?? new List<CartItem>();

            var cartVM = new CartViewModel
            {
                Items = cart,
                CartTotal = cart.Sum(c => c.Total),
                ShippingCost = 10
            };

            var defaultAddress = await _db.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault);

            var vm = new CheckoutViewModel
            {
                Cart = cartVM,
                User = user,
                Address = defaultAddress ?? new Address()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        // Thêm string paymentMethod ở đây
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model, string paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = CartHelper.GetCart(HttpContext.Session) ?? new List<CartItem>();

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty!");
                return View("Index", model);
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var address = model.Address ?? new Address();

                    var order = new Order
                    {
                        UserId = user.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.Sum(c => c.Total),
                        FullName = $"{user.FirstName} {user.LastName}",
                        PhoneNumber = user.PhoneNumber,
                        Street = address.Street ?? "",
                        Ward = address.Ward ?? "",
                        District = address.District ?? "",
                        City = address.City ?? "",
                        orderStatus = Order.OrderStatus.Pending,

                        // --- SỬA DÒNG NÀY ---
                        // Nếu khách chọn MoMo/VNPAY và đã nhấn "Tôi đã thanh toán", ta cho thành Paid luôn.
                        paymentStatus = (paymentMethod == "COD")
                                        ? Order.PaymentStatus.Unpaid
                                        : Order.PaymentStatus.Paid,

                        OrderItems = new List<OrderItem>()
                    };

                    // ... (Giữ nguyên đoạn foreach trừ kho của bạn) ...
                    foreach (var item in cart)
                    {
                        var stock = await _db.ProductSizes
                            .FirstOrDefaultAsync(ps => ps.ProductId == item.ProductId && ps.SizeName == item.Size);

                        if (stock == null || stock.Inventory < item.Quantity)
                        {
                            throw new Exception($"Product {item.ProductName} (Size {item.Size}) is out of stock!");
                        }

                        stock.Inventory -= item.Quantity;

                        var inventoryLog = new Inventory
                        {
                            ProductId = item.ProductId,
                            QuantityChanged = item.Quantity,
                            ChangeType = Inventory.ChangeLogType.Removal,
                            ChangeDate = DateTime.Now,
                            Notes = $"Order checkout #{order.Id} - Size: {item.Size} - Method: {paymentMethod}"
                        };
                        _db.Inventories.Add(inventoryLog);

                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            PriceAtPurchase = item.Price,
                            Size = item.Size
                        });
                    }

                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    CartHelper.ClearCart(HttpContext.Session);
                    return RedirectToAction("Success", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", ex.Message);
                    model.Cart = new CartViewModel { Items = cart, CartTotal = cart.Sum(c => c.Total) };
                    return View("Index", model);
                }
            }
        }

        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order); // view Success.cshtml
        }
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy tất cả đơn hàng của User này, sắp xếp mới nhất lên đầu
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // Bạn sẽ cần tạo thêm file View OrderHistory.cshtml
        }
        [HttpPost]
        public IActionResult GetPaymentQR(string method, decimal amount)
        {
            // Nội dung chuyển khoản mẫu (Thực tế sẽ là link từ MoMo/VNPAY)
            string payInfo = $"Chuyen khoan {method} - So tien: {amount}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payInfo, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrImage = qrCode.GetGraphic(20);
                return Json(new
                {
                    success = true,
                    qrCode = "data:image/png;base64," + Convert.ToBase64String(qrImage)
                });
            }
        }
    }
}
