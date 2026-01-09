using ClothesShop.Data;
using ClothesShop.Models;
using ClothesShop.Areas.Admin.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")] // Cả hai đều có quyền vào các mục này
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _db.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new ProductViewModel // Chuyển đổi sang ViewModel tại đây
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A", // Hiện tên thay vì ID
                    ProductImageIds = p.ProductImages.Select(pi => pi.Id).ToList()
                })
                .ToListAsync();

            return View(products); // Bây giờ List đã khớp với @model trong View
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Product
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Map dữ liệu từ Model sang ViewModel
            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                ProductImageIds = product.ProductImages.Select(pi => pi.Id).ToList()
            };

            return View(vm); // Khớp với @model ProductViewModel trong trang Details.cshtml
        }

        // GET: Admin/Products/Create
        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDown();
            return View(new ProductViewModel());
        }


        // POST: Admin/Products/Create
        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            // ❗ DEBUG CHẮC CHẮN
            if (!ModelState.IsValid)
            {
                // 🔥 BẮT BUỘC: truyền selectedValue
                await PopulateCategoriesDropDown(vm.CategoryId);
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name!,
                Description = vm.Description!,
                Price = vm.Price!.Value,
                CategoryId = vm.CategoryId!.Value
            };

            _db.Product.Add(product);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Product
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ProductImageIds = product.ProductImages.Select(pi => pi.Id).ToList()
            };

            await PopulateCategoriesDropDown();
            return View(vm);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropDown();
                return View(vm);
            }

            var product = await _db.Product.FindAsync(vm.Id);
            if (product == null) return NotFound();

            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price.Value;
            product.CategoryId = vm.CategoryId.Value;

            _db.Product.Update(product);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryName = product.Category?.Name,
                Price = product.Price
            };

            return View(vm);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Product.FindAsync(id);
            if (product != null)
            {
                _db.Product.Remove(product);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/GetProductsByCategory (JSON endpoint)
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = await _db.Product
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return Json(products);
        }

        // Private helper to populate category dropdown
        private async Task PopulateCategoriesDropDown(int? selectedCategoryId = null)
        {
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CategoryList = new SelectList(
                categories,
                "Id",
                "Name",
                selectedCategoryId
            );
        }
        [HttpGet]
        // Thay vì productId, hãy dùng id để khớp với asp-route-id từ View gửi sang
        [HttpGet]
        public IActionResult ManageSizes(int id) // Sử dụng 'id' để khớp với nút bấm
        {
            var product = _db.Product
                .Include(p => p.ProductSizes)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            // Truyền tên sản phẩm qua ViewBag
            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = id;

            return View(product.ProductSizes.ToList());
        }

        [HttpPost]
        public IActionResult AddSize(int productId, string sizeName, int inventory)
        {
            // Kiểm tra xem size này đã tồn tại cho sản phẩm này chưa
            var existing = _db.Set<ProductSize>()
                .FirstOrDefault(ps => ps.ProductId == productId && ps.SizeName == sizeName);

            if (existing != null)
            {
                existing.Inventory += inventory;
            }
            else
            {
                var newSize = new ProductSize
                {
                    ProductId = productId,
                    SizeName = sizeName,
                    Inventory = inventory
                };
                _db.Add(newSize);
            }

            _db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult DeleteSize(int id)
        {
            var size = _db.Set<ProductSize>().Find(id);
            if (size == null) return Json(new { success = false });

            _db.Set<ProductSize>().Remove(size);
            _db.SaveChanges();
            return Json(new { success = true });
        }
    }
}
