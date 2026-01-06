using ClothesShop.Areas.Admin.Models.ViewModel;
using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductImagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductImagesController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Admin/ProductImages
        public async Task<IActionResult> Index()
        {
            var images = await _db.ProductImages
                .Include(p => p.Product)
                .Select(img => new ProductImageVM
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    IsThumbnail = img.IsThumbnail,
                    ProductName = img.Product != null ? img.Product.Name : "N/A"
                })
                .ToListAsync();

            return View(images);
        }

        // GET: Admin/ProductImages/Create
        public async Task<IActionResult> Create()
        {
            // Tạo danh sách dropdown sản phẩm
            var products = await _db.Product.ToListAsync();
            ViewBag.ProductList = new SelectList(products, "Id", "Name");

            return View(new ProductImageVM()); // Trả về ViewModel trắng
        }
        // POST: Admin/ProductImages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductImageVM vm)
        {
            // Kiểm tra file thủ công vì IFormFile không dùng Required attribute đơn giản được
            if (vm.ImageFile == null || vm.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn một hình ảnh.");
            }

            if (ModelState.IsValid)
            {
                // 1. Xử lý Save file vật lý
                var uploadFolder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile!.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                // 2. Logic Thumbnail: Nếu set ảnh này làm thumb, thì bỏ các thumb cũ của SP đó
                if (vm.IsThumbnail)
                {
                    var oldThumbnails = await _db.ProductImages
                        .Where(x => x.ProductId == vm.ProductId && x.IsThumbnail)
                        .ToListAsync();
                    foreach (var img in oldThumbnails) img.IsThumbnail = false;
                }

                // 3. Map từ ViewModel sang Model thực thể để lưu DB
                var model = new ProductImages
                {
                    ProductId = vm.ProductId,
                    ImageUrl = "/images/products/" + fileName,
                    IsThumbnail = vm.IsThumbnail
                };

                _db.ProductImages.Add(model);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await PopulateProductsDropDown(vm.ProductId);
            return View(vm);
        }

        // GET: Admin/ProductImages/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _db.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (image == null) return NotFound();

            return View(image);
        }

        // POST: Admin/ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _db.ProductImages.FindAsync(id);

            if (image != null)
            {
                // Xóa file
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var filePath = Path.Combine(
                        _env.WebRootPath,
                        image.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _db.ProductImages.Remove(image);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // Helper
        // ===============================
        private async Task PopulateProductsDropDown(int? selectedId = null)
        {
            ViewBag.ProductList = new SelectList(
                await _db.Product.ToListAsync(),
                "Id",
                "Name",
                selectedId
            );
        }
    }
}
