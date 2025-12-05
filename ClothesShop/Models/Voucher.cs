namespace ClothesShop.Models
{
    public class Voucher
    {
        #region Status Enum
        public enum VoucherStatus
        {
            Active=1,
            Inactive=2,
            Expired=3,
            Redeemed=4
        }
        #endregion
        public int Id { get; set; }
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public int MinOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VoucherStatus Status { get; set; }
    }
}
