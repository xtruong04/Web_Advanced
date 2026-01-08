using ClothesShop.Models;

namespace ClothesShop.Areas.Admin.Models.ViewModel
{
    public class UserListViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
