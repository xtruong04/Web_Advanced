using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Areas.Admin.Models.ViewModel
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product name is required")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }
        [Required(ErrorMessage = "Please select a category")]
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<int> ProductImageIds { get; set; } = new();
    }
}
