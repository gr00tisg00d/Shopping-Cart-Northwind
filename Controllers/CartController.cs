using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[Authorize]
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

    public IActionResult Checkout()
    {
        var customer = _dataContext.Customers.FirstOrDefault(c => c.Email == User.Identity.Name);
        if (customer == null) return RedirectToAction("Login", "Account");

        var cartItems = _dataContext.CartItems
            .Include(c => c.Product)
            .Where(c => c.CustomerId == customer.CustomerId)
            .ToList();

        if (!cartItems.Any()) return RedirectToAction("Index");

        var productIds = cartItems.Select(c => c.ProductId).ToList();
        var activeDiscounts = _dataContext.Discounts
            .Where(d => productIds.Contains(d.ProductId) && d.StartTime <= DateTime.Now && d.EndTime > DateTime.Now)
            .ToDictionary(d => d.ProductId, d => d);

        ViewBag.ActiveDiscounts = activeDiscounts;
        ViewBag.Customer = customer;
        return View(cartItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlaceOrder()
    {
        var customer = _dataContext.Customers.FirstOrDefault(c => c.Email == User.Identity.Name);
        if (customer == null) return RedirectToAction("Login", "Account");

        var cartItems = _dataContext.CartItems
            .Include(c => c.Product)
            .Where(c => c.CustomerId == customer.CustomerId)
            .ToList();

        if (!cartItems.Any()) return RedirectToAction("Index");

        var productIds = cartItems.Select(c => c.ProductId).ToList();
        var activeDiscounts = _dataContext.Discounts
            .Where(d => productIds.Contains(d.ProductId) && d.StartTime <= DateTime.Now && d.EndTime > DateTime.Now)
            .ToDictionary(d => d.ProductId, d => d);

        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Customer = customer.CompanyName,
            OrderDate = DateTime.Now,
            ShipName = customer.CompanyName,
            ShipAddress = customer.Address,
            ShipCity = customer.City,
            ShipRegion = customer.Region,
            ShipPostalCode = customer.PostalCode,
            ShipCountry = customer.Country,
            Freight = 0
        };

        _dataContext.Orders.Add(order);
        _dataContext.SaveChanges();

        foreach (var item in cartItems)
        {
            decimal discount = activeDiscounts.ContainsKey(item.ProductId)
                ? activeDiscounts[item.ProductId].DiscountPercent
                : 0;

            _dataContext.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                UnitPrice = item.Product.UnitPrice,
                Quantity = (short)item.Quantity,
                Discount = discount
            });
        }

        _dataContext.CartItems.RemoveRange(cartItems);
        _dataContext.SaveChanges();

        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
    }

    public IActionResult OrderConfirmation(int orderId)
    {
        var order = _dataContext.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null) return NotFound();
        return View(order);
    }
}
