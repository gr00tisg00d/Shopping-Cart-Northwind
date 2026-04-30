using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

 [Authorize(Roles = "northwind-customer")]
 public class CartController : Controller
{
    private readonly DataContext _dataContext;

    public CartController(DataContext db)
    {
        _dataContext = db;
    }

    public IActionResult Index()
    {
        var customer = _dataContext.Customers.FirstOrDefault(c => c.Email == User.Identity.Name);

        if (customer == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _dataContext.CartItems.Include(c => c.Product).Where(c => c.CustomerId == customer.CustomerId).ToList();

        return View(cartItems);
    }
}