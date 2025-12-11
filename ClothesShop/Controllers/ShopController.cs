using ClothesShop.Data;
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

    }
}
