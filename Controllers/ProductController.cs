using Microsoft.AspNetCore.Mvc;

public class ProductController : Controller
{
  // this controller depends on the NorthwindRepository
  private DataContext _dataContext;
  public ProductController(DataContext db) => _dataContext = db;
  public IActionResult Category() => View(_dataContext.Categories.OrderBy(c => c.CategoryName));
  public IActionResult Index(int id)
  {
    ViewBag.id = id;
    ViewBag.isCustomer = User?.Identity?.IsAuthenticated == true
      && _dataContext.Customers.Any(c => c.Email == User.Identity.Name);
    return View(_dataContext.Categories.OrderBy(c => c.CategoryName));
  }
}