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
        // GET: Admin/ProductImages/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _db.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (image == null) return NotFound();

            // Map sang ViewModel để hiển thị đẹp và tránh lỗi Mismatch
            var vm = new ProductImageVM
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                IsThumbnail = image.IsThumbnail,
                ProductName = image.Product?.Name
            };

            return View(vm);
        }

        // POST: Admin/ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _db.ProductImages.FindAsync(id);

            if (image != null)
            {
                int productId = image.ProductId;
                bool wasThumbnail = image.IsThumbnail;

                // 1. Xóa file vật lý trên server
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // 2. Xóa bản ghi trong Database
                _db.ProductImages.Remove(image);
                await _db.SaveChangesAsync();

                // 3. Logic bổ sung: Nếu ảnh vừa xóa là Thumbnail, hãy chọn ảnh khác thay thế
                if (wasThumbnail)
                {
                    var nextImage = await _db.ProductImages
                        .FirstOrDefaultAsync(img => img.ProductId == productId);

                    if (nextImage != null)
                    {
                        nextImage.IsThumbnail = true;
                        _db.ProductImages.Update(nextImage);
                        await _db.SaveChangesAsync();
                    }
                }
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
        // GET: Admin/ProductImages/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var image = await _db.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (image == null) return NotFound();

            // Map từ Model sang ViewModel
            var vm = new ProductImageVM
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                IsThumbnail = image.IsThumbnail,
                ProductName = image.Product?.Name
            };

            await PopulateProductsDropDown(vm.ProductId);
            return View(vm);
        }

        // POST: Admin/ProductImages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductImageVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var imageFromDb = await _db.ProductImages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (imageFromDb == null) return NotFound();

                    string fileName = imageFromDb.ImageUrl; // Giữ lại đường dẫn cũ mặc định

                    // 1. Nếu người dùng chọn file ảnh mới
                    if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                    {
                        // Xóa ảnh cũ vật lý
                        var oldPath = Path.Combine(_env.WebRootPath, imageFromDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }

                        // Lưu ảnh mới
                        var uploadFolder = Path.Combine(_env.WebRootPath, "images/products");
                        var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                        var newFilePath = Path.Combine(uploadFolder, newFileName);

                        using (var stream = new FileStream(newFilePath, FileMode.Create))
                        {
                            await vm.ImageFile.CopyToAsync(stream);
                        }
                        fileName = "/images/products/" + newFileName;
                    }

                    // 2. Xử lý Logic Thumbnail (nếu chọn cái này làm thumb thì các cái khác của SP đó phải thôi)
                    if (vm.IsThumbnail)
                    {
                        var otherThumbnails = await _db.ProductImages
                            .Where(x => x.ProductId == vm.ProductId && x.Id != id && x.IsThumbnail)
                            .ToListAsync();
                        foreach (var img in otherThumbnails)
                        {
                            img.IsThumbnail = false;
                            _db.ProductImages.Update(img);
                        }
                    }

                    // 3. Cập nhật Model thực thể
                    var modelToUpdate = new ProductImages
                    {
                        Id = vm.Id,
                        ProductId = vm.ProductId,
                        ImageUrl = fileName,
                        IsThumbnail = vm.IsThumbnail
                    };

                    _db.ProductImages.Update(modelToUpdate);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.ProductImages.Any(e => e.Id == vm.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateProductsDropDown(vm.ProductId);
            return View(vm);
        }
    }
}
