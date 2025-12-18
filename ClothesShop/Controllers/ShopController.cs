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
        public async Task<IActionResult> Index()
        {
            var products = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return View(products);
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
