using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ClothesShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;


        // Chỉ dùng để hiển thị → không lưu DB
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();


        // Sửa DateOnly -> DateTime? để EF Core lưu được
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }


        // AvatarUrl cần lưu DB nên BỎ NotMapped
        [MaxLength(300)]
        public string? AvatarUrl { get; set; }


        // OK: Không lưu DB, chỉ để hiển thị
        [NotMapped]
        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;
                var today = DateTime.UtcNow.Date;
                var age = today.Year - DateOfBirth.Value.Year;
                if (today < DateOfBirth.Value.AddYears(age)) age--;
                return age;
            }
        }

        [MaxLength(250)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
