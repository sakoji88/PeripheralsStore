using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Helpers;
using PeripheralsStore.Models;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _dbContext;
    private const int PageSize = 6;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(ProductFilterViewModel filter)
    {
        var query = _dbContext.Products.Include(p => p.Brand).Include(p => p.Category).Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(p => p.Name.Contains(search) || p.Article.Contains(search));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.BrandId.HasValue)
            query = query.Where(p => p.BrandId == filter.BrandId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.InStockOnly)
            query = query.Where(p => p.StockQuantity > 0);

        query = filter.SortBy switch
        {
            "priceAsc" => query.OrderBy(p => p.Price),
            "priceDesc" => query.OrderByDescending(p => p.Price),
            "nameDesc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        filter.Page = Math.Max(1, filter.Page);

        ViewBag.Categories = new SelectList(await _dbContext.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", filter.CategoryId);
        ViewBag.Brands = new SelectList(await _dbContext.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", filter.BrandId);
        ViewBag.Filter = filter;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        var products = await query.Skip((filter.Page - 1) * PageSize).Take(PageSize).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _dbContext.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Reviews.Where(r => r.IsApproved)).ThenInclude(r => r.User).ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product is null)
        {
            TempData["Error"] = "Товар не найден";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.AverageRating = product.Reviews.Any() ? Math.Round(product.Reviews.Average(r => r.Rating), 1) : 0;
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await FillSelections();
        return View(new ProductCreateEditViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillSelections();
            return View(model);
        }

        NormalizeProductModel(model);

        var product = new Product
        {
            Name = model.Name,
            Article = model.Article,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            Color = model.Color,
            ConnectionType = model.ConnectionType,
            WarrantyMonths = model.WarrantyMonths,
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            CategoryId = model.CategoryId,
            BrandId = model.BrandId,
            IsActive = model.IsActive
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Товар добавлен";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await FillSelections();
        return View(new ProductCreateEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Article = product.Article,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Color = product.Color,
            ConnectionType = product.ConnectionType,
            WarrantyMonths = product.WarrantyMonths,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            IsActive = product.IsActive
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductCreateEditViewModel model)
    {
        if (!ModelState.IsValid || model.Id is null)
        {
            await FillSelections();
            return View(model);
        }

        var product = await _dbContext.Products.FindAsync(model.Id.Value);
        if (product is null)
        {
            return NotFound();
        }

        NormalizeProductModel(model);

        product.Name = model.Name;
        product.Article = model.Article;
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.Color = model.Color;
        product.ConnectionType = model.ConnectionType;
        product.WarrantyMonths = model.WarrantyMonths;
        product.Description = model.Description;
        product.ImageUrl = model.ImageUrl;
        product.CategoryId = model.CategoryId;
        product.BrandId = model.BrandId;
        product.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Товар обновлён";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _dbContext.Products.Include(p => p.Brand).Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Товар удалён";
        return RedirectToAction(nameof(Index));
    }

    private async Task FillSelections()
    {
        ViewBag.Categories = new SelectList(await _dbContext.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.Brands = new SelectList(await _dbContext.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
    }

    private static void NormalizeProductModel(ProductCreateEditViewModel model)
    {
        model.Name = InputSanitizer.Normalize(model.Name) ?? string.Empty;
        model.Article = InputSanitizer.Normalize(model.Article) ?? string.Empty;
        model.Color = InputSanitizer.Normalize(model.Color);
        model.ConnectionType = InputSanitizer.Normalize(model.ConnectionType);
        model.Description = InputSanitizer.Normalize(model.Description);
        model.ImageUrl = InputSanitizer.Normalize(model.ImageUrl);
    }
}
