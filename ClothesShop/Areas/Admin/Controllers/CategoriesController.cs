using ClothesShop.Data;
using ClothesShop.Models;
using ClothesShop.Areas.Admin.Models.ViewModel; // Đảm bảo đã khai báo đúng namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")] // Cả hai đều có quyền vào các mục này
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            // Map từ Model sang ViewModel để hiển thị (Nếu cần)
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Map từ ViewModel sang Model thực tế
            var category = new Category
            {
                Name = vm.Name,
                Description = vm.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Chuyển dữ liệu sang ViewModel
            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(vm);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var category = await _db.Categories.FindAsync(vm.Id);
            if (category == null) return NotFound();

            // Cập nhật giá trị
            category.Name = vm.Name;
            category.Description = vm.Description;

            _db.Categories.Update(category);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Delete/5
        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Tìm đối tượng Category trong DB
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // CHUYỂN ĐỔI SANG VIEWMODEL (Bắt buộc để khớp với @model trong View)
            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(vm); // Truyền vm (kiểu CategoryViewModel) vào View
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category != null)
            {
                // Kiểm tra xem có sản phẩm nào thuộc Category này không trước khi xóa (Ràng buộc dữ liệu)
                var hasProducts = await _db.Product.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    ModelState.AddModelError("", "Không thể xóa danh mục này vì vẫn còn sản phẩm bên trong.");
                    var vm = new CategoryViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description
                    };

                    return View("Delete", vm); // Trả về vm kiểu CategoryViewModel
                }

                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Tìm Category trong Database theo ID
            var category = await _db.Categories.FindAsync(id);

            // Nếu không tìm thấy, trả về trang lỗi 404
            if (category == null)
            {
                return NotFound();
            }

            // Chuyển đổi dữ liệu sang ViewModel để truyền ra View
            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(vm);
        }
    }
}