using System.ComponentModel.DataAnnotations;

namespace ClothesShop.ViewModels
{
    public class CreateAddressVM
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ đường")]
        [MaxLength(250)]
        public string Street { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Ward { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }
}
