namespace ClothesShop.Models
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; }
        public ApplicationUser User { get; set; }
        public Address Address { get; set; }
    }
}
