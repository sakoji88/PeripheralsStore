using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _dbContext;

    public AdminController(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.UsersCount = await _dbContext.Users.CountAsync();
        ViewBag.ProductsCount = await _dbContext.Products.CountAsync();
        ViewBag.ReviewsCount = await _dbContext.Reviews.CountAsync();
        ViewBag.PendingReviewsCount = await _dbContext.Reviews.CountAsync(r => !r.IsApproved);
        return View();
    }

    public async Task<IActionResult> Users(string? search)
    {
        var query = _dbContext.Users.Include(u => u.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search.Trim()) || u.Email.Contains(search.Trim()));

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserManagementViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role!.Name,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt
            }).ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Reviews()
    {
        var reviews = await _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(reviews);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.IsBanned = true;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unban(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.IsBanned = false;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        var adminRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Admin");
        if (user is null) return NotFound();
        user.RoleId = adminRole.Id;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }
}
