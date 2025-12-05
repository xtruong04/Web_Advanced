using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }
        public required string ImageUrl { get; set; }
        public bool IsThumbnail { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
