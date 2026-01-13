using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothesShop.Services;

namespace ClothesShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IMomoService _momoService;

        public CheckoutController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IMomoService momoService)
        {
            _userManager = userManager;
            _db = db;
            _momoService = momoService;
        }

        // ... (Giữ nguyên Index) ...
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // ... (Code cũ của bạn giữ nguyên) ...
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            var cart = CartHelper.GetCart(HttpContext.Session) ?? new List<CartItem>();
            var cartVM = new CartViewModel
            {
                Items = cart,
                CartTotal = cart.Sum(c => c.Total),
                ShippingCost = 10
            };
            var defaultAddress = await _db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault);
            var vm = new CheckoutViewModel { Cart = cartVM, User = user, Address = defaultAddress ?? new Address() };
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
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

                        // --- SỬA 1: LUÔN LÀ UNPAID KHI MỚI TẠO ---
                        // Dù là MoMo hay COD thì lúc này tiền chưa về túi, nên để Unpaid
                        paymentStatus = Order.PaymentStatus.Unpaid,

                        OrderItems = new List<OrderItem>()
                    };

                    // ... (Đoạn logic thêm OrderItems và trừ kho giữ nguyên) ...
                    foreach (var item in cart)
                    {
                        var stock = await _db.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductId == item.ProductId && ps.SizeName == item.Size);
                        if (stock == null || stock.Inventory < item.Quantity) throw new Exception($"Product {item.ProductName} is out of stock!");

                        stock.Inventory -= item.Quantity;
                        // ... (Add Inventory Log) ...

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

                    // --- SỬA 2: ĐIỀU HƯỚNG DỰA TRÊN PHƯƠNG THỨC THANH TOÁN ---
                    if (paymentMethod == "MOMO")
                    {
                        // Nếu là MoMo -> Chuyển hướng sang Action xử lý thanh toán
                        return RedirectToAction("CreatePaymentUrl", new { orderId = order.Id });
                    }

                    // Nếu là COD -> Chuyển thẳng đến trang Success
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

        // --- ĐÃ XÓA HÀM GetPaymentQR (VÌ DƯ THỪA VÀ GÂY LỖI) ---

        [HttpGet]
        public async Task<IActionResult> CreatePaymentUrl(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Gọi Service MoMo (Lúc này Interface đã nhận Order nên không lỗi)
            var response = await _momoService.CreatePaymentAsync(order);

            if (response != null && response.resultCode == 0)
            {
                return Redirect(response.payUrl); // Chuyển sang MoMo
            }

            string errorMessage = response != null ? response.message : "Không nhận được phản hồi từ MoMo (Response is null)";
            string debugInfo = $"LỖI THANH TOÁN MOMO:<br>" +
                               $"Message: {errorMessage}<br>" +
                               $"ResultCode: {response?.resultCode}<br>" +
                               $"Order ID: {orderId}";

            return Content(debugInfo, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> MomoReturn(MomoResultRequest response)
        {
            // 1. Kiểm tra kết quả từ MoMo
            if (response.resultCode == 0) // Giao dịch thành công
            {
                // 2. Tách lấy OrderId gốc (Bỏ đuôi thời gian _ticks đi)
                var orderIdStr = response.orderId.Split('_')[0];
                var orderId = int.Parse(orderIdStr);

                // 3. Tìm đơn hàng trong Database
                var order = await _db.Orders.FindAsync(orderId);

                // 4. CẬP NHẬT TRẠNG THÁI (Đoạn này bạn đang thiếu)
                if (order != null && order.paymentStatus != Order.PaymentStatus.Paid)
                {
                    order.paymentStatus = Order.PaymentStatus.Paid; // Đánh dấu đã trả tiền
                    order.MomoTransId = response.transId.ToString();
                    await _db.SaveChangesAsync(); // Lưu vào SQL
                }

                TempData["Success"] = "Thanh toán MoMo thành công!";
                return RedirectToAction("Success", new { id = orderId });
            }
            else
            {
                TempData["Error"] = "Thanh toán thất bại hoặc bị hủy!";
                // Xử lý logic nếu hủy (có thể quay lại trang checkout)
                return RedirectToAction("OrderHistory");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MomoNotify([FromBody] MomoResultRequest response)
        {
            if (response != null && response.resultCode == 0)
            {
                // ✅ MỚI: Cắt chuỗi lấy ID gốc trước khi tìm trong DB
                var orderIdStr = response.orderId.Split('_')[0];
                var orderIdInt = int.Parse(orderIdStr);

                var order = await _db.Orders.FindAsync(orderIdInt);

                if (order != null && order.paymentStatus != Order.PaymentStatus.Paid)
                {
                    order.paymentStatus = Order.PaymentStatus.Paid;
                    order.MomoTransId = response.transId.ToString();
                    await _db.SaveChangesAsync();
                }
            }
            return NoContent();
        }

        // ... (Giữ nguyên Success và OrderHistory) ...
        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();
            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }
    }
}