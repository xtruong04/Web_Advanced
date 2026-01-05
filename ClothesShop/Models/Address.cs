using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ClothesShop.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Added missing semicolon  

        [ForeignKey(nameof(UserId))] // Correct usage of ForeignKey attribute  
        public ApplicationUser User { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Street { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Ward { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
