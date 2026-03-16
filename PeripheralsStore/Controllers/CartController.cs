using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Models;

namespace PeripheralsStore.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly AppDbContext _dbContext;

    public CartController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();

        ViewBag.Total = items.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        quantity = Math.Clamp(quantity, 1, 100);

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
        if (product is null)
        {
            TempData["Error"] = "Товар не найден";
            return RedirectToAction("Index", "Products");
        }

        var existing = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
        if (existing is null)
        {
            _dbContext.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
        }
        else
        {
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, 100);
        }

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Товар добавлен в корзину";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item is null)
        {
            return NotFound();
        }

        item.Quantity = Math.Clamp(quantity, 1, 100);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Количество обновлено";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var items = await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (!items.Any())
        {
            TempData["Error"] = "Корзина пуста";
            return RedirectToAction(nameof(Index));
        }

        foreach (var item in items)
        {
            if (item.Product is null || item.Product.StockQuantity < item.Quantity)
            {
                TempData["Error"] = $"Недостаточно товара на складе: {item.Product?.Name ?? "Неизвестный товар"}";
                return RedirectToAction(nameof(Index));
            }
        }

        foreach (var item in items)
        {
            item.Product!.StockQuantity -= item.Quantity;
        }

        _dbContext.CartItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Покупка оформлена (демо). Остатки товаров обновлены.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item is null)
        {
            return NotFound();
        }

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Товар удалён из корзины";
        return RedirectToAction(nameof(Index));
    }
}
