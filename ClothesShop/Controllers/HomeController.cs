using System;
using System.Diagnostics;
using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                //List<Category> categories = new List<Category>();
                //categories = _context.Categories.ToList();

                List<Product> allproducts = new List<Product>();
                allproducts = _context.Product
                    .Take(8)
                    .OrderByDescending(s => s.Id).ToList();

                List<Product> featuredproducts = new List<Product>();
                featuredproducts = _context.Product
                    .Take(4)
                    .OrderByDescending(s => s.Id).ToList();
                Home item = new Home();
                //item.Categories = categories;
                item.AllProducts = allproducts;
                item.FeaturedProducts = featuredproducts;
                ViewBag.CartCount = CartHelper.GetCartCount(HttpContext.Session);

                return View(item);
            }
            catch
            {
                return Redirect("/not-found");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Blog()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
