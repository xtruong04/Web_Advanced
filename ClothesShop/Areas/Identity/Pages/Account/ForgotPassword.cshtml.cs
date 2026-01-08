// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ClothesShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ClothesShop.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Đừng tiết lộ user không tồn tại để bảo mật
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // 1. Tạo mã OTP 6 số
                Random rnd = new Random();
                string otp = rnd.Next(100000, 999999).ToString();

                // 2. Lưu OTP vào User Token (Hết hạn sau 15-30p tùy cấu hình)
                await _userManager.SetAuthenticationTokenAsync(user, "Default", "PasswordResetOTP", otp);

                // 3. Gửi Email chứa mã OTP
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password OTP",
                    $"Mã xác nhận của bạn là: <b style='font-size:20px;'>{otp}</b>");

                // 4. Chuyển hướng sang trang nhập OTP (Bạn cần Scaffold thêm trang ResetPassword)
                return RedirectToPage("./ResetPassword", new { email = Input.Email });
            }

            return Page();
        }
    }
}
