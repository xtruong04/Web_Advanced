namespace ClothesShop.Models
{
    public class Order
    {
        #region OrderStatus
        public enum OrderStatus
        {
            Pending,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        }
        #endregion
        public int Id { get; set; }
        public required string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ShippingAddress { get; set; }
        public OrderStatus orderStatus { get; set; } = OrderStatus.Pending;
        public string? PaymentStatus { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
    }
    
}
