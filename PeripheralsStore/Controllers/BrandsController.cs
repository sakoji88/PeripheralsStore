using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Helpers;
using PeripheralsStore.Models;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

[Authorize(Roles = "Admin")]
public class BrandsController : Controller
{
    private readonly AppDbContext _dbContext;

    public BrandsController(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _dbContext.Brands.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(b => b.Name.Contains(search.Trim()));
        return View(await query.OrderBy(b => b.Name).ToListAsync());
    }

    public IActionResult Create() => View(new BrandCreateEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandCreateEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        _dbContext.Brands.Add(new Brand { Name = InputSanitizer.Normalize(model.Name) ?? string.Empty, Country = InputSanitizer.Normalize(model.Country) });
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Бренд добавлен";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _dbContext.Brands.FindAsync(id);
        if (brand is null) return NotFound();
        return View(new BrandCreateEditViewModel { Id = brand.Id, Name = brand.Name, Country = brand.Country });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BrandCreateEditViewModel model)
    {
        if (!ModelState.IsValid || model.Id is null) return View(model);
        var brand = await _dbContext.Brands.FindAsync(model.Id.Value);
        if (brand is null) return NotFound();
        brand.Name = InputSanitizer.Normalize(model.Name) ?? string.Empty;
        brand.Country = InputSanitizer.Normalize(model.Country);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Бренд обновлен";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _dbContext.Brands.FindAsync(id);
        if (brand is null) return NotFound();
        return View(brand);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var brand = await _dbContext.Brands.FindAsync(id);
        if (brand is null) return NotFound();
        _dbContext.Brands.Remove(brand);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Бренд удален";
        return RedirectToAction(nameof(Index));
    }
}
