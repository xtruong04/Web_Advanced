using ClothesShop.Models; // Các model của bạn
using System.Collections.Generic;

namespace ClothesShop.Areas.Admin.Models.ViewModel
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
    }
}
