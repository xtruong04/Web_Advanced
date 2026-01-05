using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _userManager = userManager;
            _env = env;
            _context = context;
        }

        // ============================
        // GET: PROFILE
        // ============================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            // Nếu bạn có bảng Address thì lấy địa chỉ mặc định
            var defaultAddress = await _context.Addresses
                .Where(a => a.UserId == user.Id && a.IsDefault)
                .FirstOrDefaultAsync();

            // Tạo ViewModel
            var vm = new ProfileViewModel
            {
                User = user,
                DefaultAddress = defaultAddress
            };

            return View(vm);
        }


        // ============================
        // POST: UPDATE PROFILE
        // ============================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            var vm = new ProfileViewModel
            {
                AvatarUrl = user.AvatarUrl,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            // Upload avatar
            if (model.AvatarFile != null)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.AvatarFile.FileName)}";
                string path = Path.Combine("wwwroot/avatar", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = fileName;
            }

            await _userManager.UpdateAsync(user);

            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }


    }
}
