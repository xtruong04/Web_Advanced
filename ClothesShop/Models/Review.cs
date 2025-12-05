namespace ClothesShop.Models
{
    public class Review
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int ProductId { get; set; }
        public string Rating { get; set; } = string.Empty;
        public required string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
