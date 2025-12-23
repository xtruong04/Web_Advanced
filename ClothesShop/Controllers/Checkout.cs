using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.Controllers
{
    public class Checkout : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
