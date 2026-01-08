// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ClothesShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ClothesShop.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Mật khẩu phải dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }

            // Sửa trường Code thành OTP để nhập mã 6 số
            [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
            [StringLength(6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số.", MinimumLength = 6)]
            [Display(Name = "Mã xác nhận (OTP)")]
            public string OTP { get; set; }
        }

        public IActionResult OnGet(string email = null)
        {
            if (email == null)
            {
                return BadRequest("Email phải được cung cấp để đặt lại mật khẩu.");
            }
            else
            {
                Input = new InputModel
                {
                    Email = email
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null) return RedirectToPage("./ResetPasswordConfirmation");

            // 1. Lấy mã OTP đã lưu trong DB
            var savedOtp = await _userManager.GetAuthenticationTokenAsync(user, "Default", "PasswordResetOTP");

            if (Input.OTP == savedOtp)
            {
                // 2. OTP đúng -> Tạo ResetToken thật của Identity để đổi mật khẩu
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, Input.Password);

                if (result.Succeeded)
                {
                    // 3. Xóa OTP sau khi dùng xong
                    await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "PasswordResetOTP");
                    return RedirectToPage("./ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không chính xác hoặc đã hết hạn.");
            }

            return Page();
        }
    }
}
