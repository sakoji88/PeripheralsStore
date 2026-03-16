using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;

namespace PeripheralsStore.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _dbContext;

    public HomeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var popularProducts = await _dbContext.Products
            .Where(p => p.IsActive)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.StockQuantity)
            .Take(6)
            .ToListAsync();

        return View(popularProducts);
    }

    public IActionResult Error()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        TempData["Error"] = "У вас недостаточно прав для выполнения этого действия";
        return RedirectToAction(nameof(Index));
    }
}
