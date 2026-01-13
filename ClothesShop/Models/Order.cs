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

        public enum PaymentStatus
        {
            Unpaid,
            Paid,
            Refunded
        }
        #endregion
        public int Id { get; set; }
        public string? MomoTransId { get; set; } // Lưu mã giao dịch của MoMo (VD: 2304912349)
        public required string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public OrderStatus orderStatus { get; set; } = OrderStatus.Pending;
        public PaymentStatus paymentStatus { get; set; } = PaymentStatus.Unpaid;
        public List<OrderItem>? OrderItems { get; set; }
    }
    
}
