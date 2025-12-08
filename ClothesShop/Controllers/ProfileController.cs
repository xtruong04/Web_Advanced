using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        // ============================
        // GET: PROFILE
        // ============================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            return View(user); // View yêu cầu ApplicationUser
        }


        // ============================
        // POST: UPDATE PROFILE
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(IFormFile? AvatarUpload, string FullName, string PhoneNumber, string Address)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            // Cập nhật FirstName & LastName
            if (!string.IsNullOrWhiteSpace(FullName))
            {
                var parts = FullName.Trim().Split(' ', 2);
                user.FirstName = parts.Length > 0 ? parts[0] : "";
                user.LastName = parts.Length > 1 ? parts[1] : "";
            }

            user.PhoneNumber = PhoneNumber;
            user.Address = Address;

            // ============================
            // Upload Avatar
            // ============================
            if (AvatarUpload != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "avatar");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarUpload.FileName)}";
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarUpload.CopyToAsync(stream);
                }

                user.AvatarUrl = fileName;
            }

            await _userManager.UpdateAsync(user);

            TempData["success"] = "Cập nhật hồ sơ thành công!";

            return RedirectToAction("Index");
        }
    }
}
