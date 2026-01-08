using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var cartVM = new CartViewModel()
            {
                Items = cart,
                CartTotal = CartHelper.GetTotal(HttpContext.Session),
                ShippingCost = 30000
            };
            ViewBag.Title = "Cart page";
            return View(cartVM);
        }
        public PartialViewResult _Cart()
        {
            ViewBag.CartCount = CartHelper.GetCartCount(HttpContext.Session);
            return PartialView();
        }
        [HttpPost]
        public IActionResult Add(int id, string name, string image, decimal price, int quantity)
        {
            // KIỂM TRA LỖI: Nếu image chứa text "System.Collections..."
            if (string.IsNullOrEmpty(image) || image.Contains("System.Collections.Generic"))
            {
                // Truy vấn DB lấy lại ảnh đầu tiên của sản phẩm này
                var product = _context.Product
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.Id == id);

                image = product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/Content/clientpage/assets/img/default.jpg";
            }

            var item = new CartItem
            {
                ProductId = id,
                ProductName = name,
                ProductImage = image,
                Price = price,
                Quantity = quantity
            };

            CartHelper.AddToCart(HttpContext.Session, item);

            return Json(new
            {
                success = true,
                cartCount = CartHelper.GetCartCount(HttpContext.Session)
            });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                CartHelper.UpdateQuantity(HttpContext.Session, productId, quantity);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        public ActionResult Remove(int id)
        {
            try
            {
                CartHelper.RemoveFromCart(HttpContext.Session, id);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        public ActionResult Clear()
        {
            CartHelper.ClearCart(HttpContext.Session);
            TempData["Success"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng!";
            return RedirectToAction("Index");

        }
    }
}
