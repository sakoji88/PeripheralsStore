using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Helpers;
using PeripheralsStore.Models;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

public class ReviewsController : Controller
{
    private readonly AppDbContext _dbContext;

    public ReviewsController(AppDbContext dbContext) => _dbContext = dbContext;

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Проверьте корректность заполнения отзыва";
            return RedirectToAction("Details", "Products", new { id = model.ProductId });
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var exists = await _dbContext.Reviews.AnyAsync(r => r.ProductId == model.ProductId && r.UserId == userId);
        if (exists)
        {
            TempData["Error"] = "Вы уже оставляли отзыв на этот товар";
            return RedirectToAction("Details", "Products", new { id = model.ProductId });
        }

        _dbContext.Reviews.Add(new Review
        {
            ProductId = model.ProductId,
            UserId = userId,
            Rating = model.Rating,
            Comment = InputSanitizer.Normalize(model.Comment) ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false
        });

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Отзыв отправлен на модерацию";
        return RedirectToAction("Details", "Products", new { id = model.ProductId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int id, bool approved)
    {
        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        review.IsApproved = approved;
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = approved ? "Отзыв одобрен" : "Отзыв скрыт";
        return RedirectToAction("Reviews", "Admin");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Отзыв удалён";
        return RedirectToAction("Reviews", "Admin");
    }
}
