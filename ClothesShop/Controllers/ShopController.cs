using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang Shop - hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index(string? price, int? cateId)
        {
            var products = _context.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // 🔹 Category filter
            if (cateId.HasValue)
            {
                products = products.Where(p => p.CategoryId == cateId.Value);
            }

            // 🔹 Price filter
            if (!string.IsNullOrEmpty(price))
            {
                var parts = price.Split('-');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var min) &&
                    decimal.TryParse(parts[1], out var max))
                {
                    products = products.Where(p => p.Price >= min && p.Price <= max);
                }
            }

            return View(await products.ToListAsync());
        }

        public ActionResult ShopDetails(int id)
        {
            var item = _context.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (item == null)
                return Redirect("/not-found");

            return View(item);
        }

        public ActionResult GetProductByCate(int proId, int CateId)
        {
            List<Product> item = new List<Product>();
            try
            {
                item = _context.Product.Where(s => s.CategoryId == CateId && s.Id != proId).Take(4).ToList();
                return PartialView(item);
            }
            catch
            {
                item = new List<Product>();
                return PartialView(item);
            }
        }
    }
}
