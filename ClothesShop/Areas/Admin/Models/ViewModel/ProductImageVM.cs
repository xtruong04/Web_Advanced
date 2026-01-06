namespace ClothesShop.Areas.Admin.Models.ViewModel
{
    public class ProductImageVM
    {
        public int Id { get; set; } // Dùng cho Index/Delete

        public int ProductId { get; set; }

        public string? ProductName { get; set; } // Hiển thị tên SP ở Index

        public string? ImageUrl { get; set; }

        public bool IsThumbnail { get; set; }

        // Nhận file từ Form
        public IFormFile? ImageFile { get; set; }
    }
}
