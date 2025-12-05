namespace ClothesShop.Models
{
    public class Payment
    {
        #region PaymentStatus Enum
        public enum PaymentStatus
        {
            Pending,
            Completed,
            Failed,
            Refunded
        }
        #endregion

        public int Id { get; set; }
        public int OrderId { get; set; }
        public required string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; } = PaymentStatus.Pending;
    }
}
