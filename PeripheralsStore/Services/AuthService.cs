using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Data;
using PeripheralsStore.Helpers;
using PeripheralsStore.Models;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasherService _passwordHasher;

    public AuthService(AppDbContext dbContext, PasswordHasherService passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterViewModel model)
    {
        model.FullName = InputSanitizer.Normalize(model.FullName) ?? string.Empty;
        model.Email = InputSanitizer.Normalize(model.Email)?.ToLowerInvariant() ?? string.Empty;

        if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
        {
            return (false, "Пользователь с таким email уже существует");
        }

        var userRole = await _dbContext.Roles.FirstAsync(r => r.Name == "User");

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = _passwordHasher.HashPasswordDeterministic(model.Password),
            RoleId = userRole.Id,
            IsBanned = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage, User? User)> ValidateUserAsync(LoginViewModel model)
    {
        model.Email = InputSanitizer.Normalize(model.Email)?.ToLowerInvariant() ?? string.Empty;

        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user is null || !_passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            return (false, "Неверный email или пароль", null);
        }

        if (user.IsBanned)
        {
            return (false, "Ваш аккаунт заблокирован. Обратитесь к администратору", null);
        }

        return (true, string.Empty, user);
    }

    public async Task SignInAsync(HttpContext context, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? "User")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12) });
    }
}
