using System;
using System.Diagnostics;
using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var categories = _context.Categories
                    .Include(c => c.Products) // Thêm dòng này để nạp danh sách sản phẩm
                    .ToList();

                var allProducts = _context.Product
                    .Include(p => p.ProductImages)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToList();

                var featuredProducts = _context.Product
                    .Include(p => p.ProductImages)
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .ToList();

                var model = new Home
                {
                    Categories = categories,
                    AllProducts = allProducts,
                    FeaturedProducts = featuredProducts
                };

                ViewBag.CartCount = CartHelper.GetCartCount(HttpContext.Session);

                return View(model);
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
