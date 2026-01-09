using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var cartVM = new CartViewModel()
            {
                Items = cart,
                CartTotal = CartHelper.GetTotal(HttpContext.Session),
                ShippingCost = 30000
            };
            ViewBag.Title = "Cart page";
            return View(cartVM);
        }
        public PartialViewResult _Cart()
        {
            ViewBag.CartCount = CartHelper.GetCartCount(HttpContext.Session);
            return PartialView();
        }
        [HttpPost]
        public IActionResult Add(int id, string name, string image, decimal price, int quantity, string size)
        {
            // 1. Kiểm tra xem người dùng đã chọn size chưa
            if (string.IsNullOrEmpty(size))
            {
                return Json(new { success = false, message = "Vui lòng chọn size trước khi thêm!" });
            }

            // 2. Kiểm tra tồn kho (Inventory) trong DB
            var sizeInfo = _context.Set<ProductSize>()
                .FirstOrDefault(ps => ps.ProductId == id && ps.SizeName == size);

            if (sizeInfo == null || sizeInfo.Inventory < quantity)
            {
                return Json(new { success = false, message = "Sản phẩm size này đã hết hàng hoặc không đủ số lượng!" });
            }

            // 3. Xử lý ảnh lỗi như cũ
            if (string.IsNullOrEmpty(image) || image.Contains("System.Collections.Generic"))
            {
                var product = _context.Product
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.Id == id);
                image = product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/Content/clientpage/assets/img/default.jpg";
            }

            // 4. Tạo Item giỏ hàng mới (có Size)
            var item = new CartItem
            {
                ProductId = id,
                ProductName = name,
                ProductImage = image,
                Price = price,
                Quantity = quantity,
                Size = size // Gán size ở đây
            };

            CartHelper.AddToCart(HttpContext.Session, item);

            return Json(new
            {
                success = true,
                cartCount = CartHelper.GetCartCount(HttpContext.Session)
            });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, string size, int quantity) // Thêm tham số string size
        {
            CartHelper.UpdateQuantity(HttpContext.Session, productId, size, quantity);
            return RedirectToAction("Index");
        }
        public ActionResult Remove(int id, string size) // Thêm tham số string size
        {
            CartHelper.RemoveFromCart(HttpContext.Session, id, size);
            return RedirectToAction("Index");
        }
        public ActionResult Clear()
        {
            CartHelper.ClearCart(HttpContext.Session);
            TempData["Success"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng!";
            return RedirectToAction("Index");

        }
    }
}
