namespace ClothesShop.Models
{
    public class Home
    {
        public List<Category> Categories { get; set; }
        public List<Product> AllProducts { get; set; }
        public List<Product> FeaturedProducts { get; set; }
    }
}
