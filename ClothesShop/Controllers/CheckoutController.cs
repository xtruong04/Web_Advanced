using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = CartHelper.GetCart(HttpContext.Session) ?? new List<CartItem>();

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty!");
                return View("Index", model);
            }

            // Bắt đầu Transaction để bảo vệ dữ liệu
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
                        paymentStatus = Order.PaymentStatus.Unpaid,
                        OrderItems = new List<OrderItem>() // Khởi tạo danh sách trống
                    };

                    foreach (var item in cart)
                    {
                        // 1. TÌM KIẾM TRONG KHO (Bảng ProductSize lưu trữ thực tế)
                        // Lưu ý: Đảm bảo CartItem của bạn có thuộc tính .Size
                        var stock = await _db.ProductSizes
                            .FirstOrDefaultAsync(ps => ps.ProductId == item.ProductId && ps.SizeName == item.Size);

                        if (stock == null || stock.Inventory < item.Quantity)
                        {
                            // Nếu không đủ hàng, ném lỗi để nhảy vào catch
                            throw new Exception($"Product {item.ProductName} (Size {item.Size}) is out of stock or insufficient!");
                        }

                        // 2. TRỪ KHO THỰC TẾ
                        stock.Inventory -= item.Quantity;

                        // 3. GHI NHẬT KÝ BIẾN ĐỘNG (Bảng Inventory Log)
                        var inventoryLog = new Inventory
                        {
                            ProductId = item.ProductId,
                            QuantityChanged = item.Quantity,
                            ChangeType = Inventory.ChangeLogType.Removal, // Xuất kho
                            ChangeDate = DateTime.Now,
                            Notes = $"Order checkout #{order.Id} - Size: {item.Size}"
                        };
                        _db.Inventories.Add(inventoryLog);

                        // 4. THÊM VÀO CHI TIẾT ĐƠN HÀNG
                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            PriceAtPurchase = item.Price,
                            Size = item.Size // Đảm bảo OrderItem model đã có trường Size như mình thảo luận
                        });
                    }

                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();

                    // Xác nhận mọi thay đổi thành công
                    await transaction.CommitAsync();

                    CartHelper.ClearCart(HttpContext.Session);
                    return RedirectToAction("Success", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    // Nếu có bất kỳ lỗi nào, hủy bỏ toàn bộ quá trình (Rollback)
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", ex.Message);

                    // Cần chuẩn bị lại dữ liệu cho view nếu quay lại trang Index
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
    }
}
