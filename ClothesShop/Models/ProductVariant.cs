using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }
        public required string Size { get; set; }
        public required string Color { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; } = string.Empty;
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
