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
                OrderItems = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    PriceAtPurchase = c.Price
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart session
            CartHelper.ClearCart(HttpContext.Session);

            // Redirect to Success page with order Id
            return RedirectToAction("Success", new { id = order.Id });
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
