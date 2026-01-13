namespace ClothesShop.Models
{
    public class MomoCreatePaymentResponse
    {
        public int resultCode { get; set; }
        public string message { get; set; }
        public string qrCodeUrl { get; set; }
        public string payUrl { get; set; } // Link chuyển hướng sang web/app MoMo
    }
}
