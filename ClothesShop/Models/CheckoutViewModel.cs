namespace ClothesShop.Models
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new CartViewModel(); // tránh null
        public ApplicationUser User { get; set; } = new ApplicationUser(); // tránh null
        public Address Address { get; set; } = new Address(); // tránh null
    }
}
