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
        public async Task<IActionResult> Index(string? price, int? cateId, int? pageNumber)
        {
            // Lưu lại bộ lọc để giữ trạng thái khi chuyển trang trên View
            ViewData["CurrentPrice"] = price;
            ViewData["CurrentCate"] = cateId;

            var products = _context.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // 1. Lọc theo Category
            if (cateId.HasValue)
            {
                products = products.Where(p => p.CategoryId == cateId.Value);
            }

            // 2. Lọc theo Giá
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

            // 3. Cấu hình phân trang
            int pageSize = 6; // Số sản phẩm trên mỗi trang
            int pageIndex = pageNumber ?? 1;

            // Đếm tổng số sản phẩm sau khi đã lọc
            var count = await products.CountAsync();

            // Lấy dữ liệu theo trang
            var items = await products
                .OrderByDescending(p => p.Id) // Bắt buộc phải OrderBy khi dùng Skip/Take
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền các thông số phân trang qua ViewBag
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.HasPreviousPage = pageIndex > 1;
            ViewBag.HasNextPage = pageIndex < (int)Math.Ceiling(count / (double)pageSize);

            return View(items);
        }

        // ShopController.cs
        public IActionResult ShopDetails(int id)
        {
            var product = _context.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSizes) // Đảm bảo nạp danh sách Size
                .AsNoTracking() // Thêm dòng này để tăng tốc độ truy vấn (chỉ đọc)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
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
