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

        var productIds = cartItems.Select(c => c.ProductId).ToList();

        var activeDiscounts = _dataContext.Discounts.Where(d => productIds.Contains(d.ProductId) && d.StartTime <= DateTime.Now &&
           d.EndTime > DateTime.Now).ToDictionary(d => d.ProductId, d => d);

        ViewBag.ActiveDiscounts = activeDiscounts;

        return View(cartItems);
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int cartItemId, int quantity)
    {
        var item = _dataContext.CartItems.Include(c => c.Product).FirstOrDefault(c => c.CartItemId == cartItemId);

        if (item == null)
        {
            return NotFound();
        }
        if (quantity <= 0)
        {
            _dataContext.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        _dataContext.SaveChanges();

        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult RemoveItem(int cartItemId)
    {
        var item = _dataContext.CartItems.FirstOrDefault(c => c.CartItemId == cartItemId);

        if (item == null)
        {
            return NotFound();
        }

        _dataContext.CartItems.Remove(item);
        _dataContext.SaveChanges();

        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult Checkout()
    {
        var customer = _dataContext.Customers.FirstOrDefault(c => c.Email == User.Identity.Name);

        if (customer == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _dataContext.CartItems.Include(c => c.Product).Where(c => c.CustomerId == customer.CustomerId).ToList();

        if (!cartItems.Any())
        {
            return RedirectToAction("Index");
        }

        var productIds = cartItems.Select(c => c.ProductId).ToList();

        var activeDiscounts = _dataContext.Discounts.Where(d => productIds.Contains(d.ProductId) && d.StartTime <= DateTime.Now && d.EndTime > DateTime.Now)
            .GroupBy(d => d.ProductId).ToDictionary(g => g.Key, g => g.First());

        var order = new Order
        {
            CustomerId = customer.CustomerId,
            OrderDate = DateTime.Now,
            RequiredDate = DateTime.Now.AddDays(7),
            Freight = 0,

            ShipName = customer.CompanyName,
            ShipAddress = customer.Address,
            ShipCity = customer.City,
            ShipRegion = customer.Region,
            ShipPostalCode = customer.PostalCode,
            ShipCountry = customer.Country
        };

        _dataContext.Orders.Add(order);
        _dataContext.SaveChanges();

        foreach (var item in cartItems)
        {
            decimal discountPercent = 0;

            if (activeDiscounts.ContainsKey(item.ProductId))
            {
                discountPercent = activeDiscounts[item.ProductId].DiscountPercent;
            }

            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                UnitPrice = item.Product.UnitPrice,
                Quantity = Convert.ToInt16(item.Quantity),
                Discount = discountPercent
            };
            _dataContext.OrderDetails.Add(orderDetail);
        }

        _dataContext.CartItems.RemoveRange(cartItems);
        _dataContext.SaveChanges();

        TempData["SuccessMessage"] = "Your order has been placed successfully!";

        return RedirectToAction("Index");

    }
}
