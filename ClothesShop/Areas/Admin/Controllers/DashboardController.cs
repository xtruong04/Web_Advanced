using ClothesShop.Areas.Admin.Models.ViewModel;
using ClothesShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")] // Cả hai đều có quyền vào các mục này
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new DashboardViewModel
            {
                TotalOrders = _context.Orders.Count(),
                TotalSales = _context.Orders.Sum(o => o.TotalAmount),
                TotalProducts = _context.Product.Count(),
                TotalCustomers = _context.Users.Count(),

                RecentOrders = _context.Orders.Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList(),

                RecentProducts = _context.Product
                    //.OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }
    }
}
