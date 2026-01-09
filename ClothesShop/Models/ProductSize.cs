using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public string SizeName { get; set; } = string.Empty; // S, M, L, XL...

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Inventory { get; set; } // Số lượng tồn kho của size này
    }
}