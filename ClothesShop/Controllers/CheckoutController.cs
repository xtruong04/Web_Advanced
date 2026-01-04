using ClothesShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

public class CheckoutController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    [Authorize]

    public async Task<IActionResult> Index()
    {
        // CART
        var cart = CartHelper.GetCart(HttpContext.Session);

        var cartVM = new CartViewModel
        {
            Items = cart,
            CartTotal = cart.Sum(c => c.Total),
            ShippingCost = 10
        };

        // USER
        ApplicationUser currentUser = null;

        if (User.Identity.IsAuthenticated)
        {
            currentUser = await _userManager.GetUserAsync(User);
        }

        var vm = new CheckoutViewModel
        {
            Cart = cartVM,
            User = currentUser
        };

        return View(vm);
    }
}
