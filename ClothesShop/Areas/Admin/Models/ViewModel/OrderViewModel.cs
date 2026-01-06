namespace ClothesShop.Areas.Admin.Models.ViewModel
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingAddress { get; set; } // Gộp các trường địa chỉ lại
    }
}
