using ClothesShop.Data;
using ClothesShop.Models;
using ClothesShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public AddressController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // ================================
        // LIST
        // ================================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var addresses = await _db.Addresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();

            return View(addresses);
        }

        // ================================
        // CREATE
        // ================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.FullName = user.FullName;
            ViewBag.PhoneNumber = user.PhoneNumber;

            return View(new CreateAddressVM());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAddressVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var address = new Address
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Street = vm.Street,
                Ward = vm.Ward,
                District = vm.District,
                City = vm.City
            };

            if (!await _db.Addresses.AnyAsync(a => a.UserId == user.Id))
                address.IsDefault = true;

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            TempData["success"] = "Thêm địa chỉ thành công!";
            return RedirectToAction(nameof(Index));
        }


        // ================================
        // EDIT
        // ================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var addr = await _db.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (addr == null)
                return NotFound();

            return View(addr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Address model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            var addr = await _db.Addresses
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == user.Id);

            if (addr == null)
                return NotFound();

            addr.FullName = model.FullName;
            addr.PhoneNumber = model.PhoneNumber;
            addr.Street = model.Street;
            addr.Ward = model.Ward;
            addr.District = model.District;
            addr.City = model.City;

            await _db.SaveChangesAsync();

            TempData["success"] = "Cập nhật địa chỉ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // DELETE
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var addr = await _db.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (addr == null)
                return NotFound();

            bool wasDefault = addr.IsDefault;

            _db.Addresses.Remove(addr);
            await _db.SaveChangesAsync();

            if (wasDefault)
            {
                var newDefault = await _db.Addresses
                    .Where(a => a.UserId == user.Id)
                    .FirstOrDefaultAsync();

                if (newDefault != null)
                {
                    newDefault.IsDefault = true;
                    await _db.SaveChangesAsync();
                }
            }

            TempData["success"] = "Xóa địa chỉ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // SET DEFAULT
        // ================================
        [HttpGet]
        public async Task<IActionResult> SetDefault(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Lấy địa chỉ cần set mặc định (PHẢI thuộc user)
            var address = await _db.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (address == null)
                return NotFound();

            // Bỏ mặc định tất cả địa chỉ của user
            var userAddresses = await _db.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            foreach (var a in userAddresses)
            {
                a.IsDefault = false;
            }

            // Set địa chỉ mới
            address.IsDefault = true;

            await _db.SaveChangesAsync();

            TempData["success"] = "Đã đặt làm địa chỉ mặc định";
            return RedirectToAction("Index");
        }

    }
}
