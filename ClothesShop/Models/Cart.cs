using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItem>? CartItems { get; set; }
        public decimal TotalAmount
        {
            get
            {
                return CartItems?.Sum(item => item.PriceAtAddition * item.Quantity) ?? 0;
            }
        }
    }
}
