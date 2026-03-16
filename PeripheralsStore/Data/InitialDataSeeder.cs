using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Models;
using PeripheralsStore.Services;

namespace PeripheralsStore.Data;

/// <summary>
/// Сидирование только минимально обязательных данных.
/// Не подменяет пользовательский каталог из реальной БД.
/// </summary>
public static class InitialDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasherService>();

        if (!await dbContext.Roles.AnyAsync())
        {
            dbContext.Roles.AddRange(
                new Role { Name = "User" },
                new Role { Name = "Admin" });
            await dbContext.SaveChangesAsync();
        }

        var adminRole = await dbContext.Roles.FirstAsync(r => r.Name == "Admin");
        if (!await dbContext.Users.AnyAsync(u => u.Email == "admin@peripherals.local"))
        {
            dbContext.Users.Add(new User
            {
                FullName = "Администратор",
                Email = "admin@peripherals.local",
                PasswordHash = hasher.HashPasswordDeterministic("Admin123!"),
                RoleId = adminRole.Id,
                IsBanned = false,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        var seedDemoCatalog = configuration.GetValue<bool>("DatabaseSettings:SeedDemoCatalog");
        if (seedDemoCatalog)
        {
            await SeedDemoCatalogAsync(dbContext, logger);
        }
    }

    private static async Task SeedDemoCatalogAsync(AppDbContext dbContext, ILogger logger)
    {
        if (await dbContext.Products.AnyAsync())
        {
            logger.LogInformation("Демо-каталог не добавлен: товары уже есть в БД.");
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Клавиатуры", Description = "Механические и мембранные клавиатуры" },
            new Category { Name = "Мыши", Description = "Игровые и офисные мыши" },
            new Category { Name = "Наушники", Description = "Проводные и беспроводные модели" }
        };

        var brands = new[]
        {
            new Brand { Name = "Logitech", Country = "Швейцария" },
            new Brand { Name = "Razer", Country = "США" },
            new Brand { Name = "HyperX", Country = "США" }
        };

        dbContext.Categories.AddRange(categories);
        dbContext.Brands.AddRange(brands);
        await dbContext.SaveChangesAsync();

        dbContext.Products.AddRange(
            new Product
            {
                Name = "Logitech G Pro X", Article = "KB-001", Price = 10990, StockQuantity = 12,
                Color = "Черный", ConnectionType = "USB", WarrantyMonths = 24,
                Description = "Игровая механическая клавиатура", CategoryId = categories[0].Id, BrandId = brands[0].Id, IsActive = true,
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800"
            },
            new Product
            {
                Name = "Razer DeathAdder V2", Article = "MS-001", Price = 6490, StockQuantity = 25,
                Color = "Черный", ConnectionType = "USB", WarrantyMonths = 24,
                Description = "Эргономичная игровая мышь", CategoryId = categories[1].Id, BrandId = brands[1].Id, IsActive = true,
                ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?w=800"
            });

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Добавлен демонстрационный каталог товаров.");
    }
}
