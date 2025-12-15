namespace ClothesShop.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal CartTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total => CartTotal + ShippingCost;
    }
}
