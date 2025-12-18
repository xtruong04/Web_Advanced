using ClothesShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace ClothesShop.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
    }
}
