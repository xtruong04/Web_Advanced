using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.Controllers
{
    public class CartController : Controller
    {
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
            return RedirectToAction("Index");

        }
    }
}
