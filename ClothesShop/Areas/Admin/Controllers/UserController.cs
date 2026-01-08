using ClothesShop.Areas.Admin.Models.ViewModel;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào đây
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Danh sách User
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserListViewModel>();

            foreach (var user in users)
            {
                // Lấy danh sách tên Role của từng User
                var roles = await _userManager.GetRolesAsync(user);

                userRolesViewModel.Add(new UserListViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(userRolesViewModel);
        }

        // 2. Thay đổi Role (GET)
        public async Task<IActionResult> EditRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = userId;
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;

            return View();
        }

        // 3. Xử lý cập nhật Role (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Xóa hết role cũ và thêm role mới chọn
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, roles);

            return RedirectToAction(nameof(Index));
        }

        // 4. Khóa/Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
                await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(100));
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            return RedirectToAction(nameof(Index));
        }
    }
}
