using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Helpers;
using PeripheralsStore.Models;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _dbContext;

    public CategoriesController(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _dbContext.Categories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Name.Contains(search.Trim()));
        return View(await query.OrderBy(c => c.Name).ToListAsync());
    }

    public IActionResult Create() => View(new CategoryCreateEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Name = InputSanitizer.Normalize(model.Name) ?? string.Empty;
        model.Description = InputSanitizer.Normalize(model.Description);
        _dbContext.Categories.Add(new Category { Name = model.Name, Description = model.Description });
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Категория создана";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category is null) return NotFound();
        return View(new CategoryCreateEditViewModel { Id = category.Id, Name = category.Name, Description = category.Description });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryCreateEditViewModel model)
    {
        if (!ModelState.IsValid || model.Id is null) return View(model);
        var category = await _dbContext.Categories.FindAsync(model.Id.Value);
        if (category is null) return NotFound();
        category.Name = InputSanitizer.Normalize(model.Name) ?? string.Empty;
        category.Description = InputSanitizer.Normalize(model.Description);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Категория обновлена";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category is null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category is null) return NotFound();
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Категория удалена";
        return RedirectToAction(nameof(Index));
    }
}
