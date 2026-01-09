using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public List<ProductImages>? ProductImages { get; set; }
        public virtual List<ProductSize>? ProductSizes { get; set; }
    }
}
