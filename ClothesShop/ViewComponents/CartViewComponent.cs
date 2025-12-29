using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            ViewBag.CartCount = CartHelper.GetCartCount(HttpContext.Session);
            return View();
        }
    }
}