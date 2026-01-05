using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class CheckoutController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public CheckoutController(UserManager<ApplicationUser> userManager,ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        // USER
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return RedirectToAction("Login", "Account");

        // CART
        var cart = CartHelper.GetCart(HttpContext.Session);

        var cartVM = new CartViewModel
        {
            Items = cart,
            CartTotal = cart.Sum(c => c.Total),
            ShippingCost = 10
        };

        // DEFAULT ADDRESS
        var defaultAddress = await _db.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == currentUser.Id && a.IsDefault);

        // VIEWMODEL
        var vm = new CheckoutViewModel
        {
            Cart = cartVM,
            User = currentUser,
            Address = defaultAddress
        };

        return View(vm);
    }

}
