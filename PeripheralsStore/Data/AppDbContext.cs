using Microsoft.EntityFrameworkCore;
using PeripheralsStore.Models;

namespace PeripheralsStore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasIndex(ci => new { ci.UserId, ci.ProductId })
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Admin" });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = "Администратор",
            Email = "admin@peripherals.local",
            PasswordHash = "4REMrnTLBfX4zzsL7i6j9D/XA6CbA4+ngAQ3h3Sx8ao=",
            RoleId = 2,
            IsBanned = false,
            CreatedAt = new DateTime(2024, 1, 1)
        });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Клавиатуры", Description = "Механические и мембранные клавиатуры" },
            new Category { Id = 2, Name = "Мыши", Description = "Игровые и офисные мыши" },
            new Category { Id = 3, Name = "Наушники", Description = "Проводные и беспроводные модели" });

        modelBuilder.Entity<Brand>().HasData(
            new Brand { Id = 1, Name = "Logitech", Country = "Швейцария" },
            new Brand { Id = 2, Name = "Razer", Country = "США" },
            new Brand { Id = 3, Name = "HyperX", Country = "США" });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1, Name = "Logitech G Pro X", Article = "KB-001", Price = 10990, StockQuantity = 12,
                Color = "Черный", ConnectionType = "USB", WarrantyMonths = 24,
                Description = "Игровая механическая клавиатура", CategoryId = 1, BrandId = 1, IsActive = true,
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800"
            },
            new Product
            {
                Id = 2, Name = "Razer DeathAdder V2", Article = "MS-001", Price = 6490, StockQuantity = 25,
                Color = "Черный", ConnectionType = "USB", WarrantyMonths = 24,
                Description = "Эргономичная игровая мышь", CategoryId = 2, BrandId = 2, IsActive = true,
                ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?w=800"
            },
            new Product
            {
                Id = 3, Name = "HyperX Cloud II", Article = "HP-001", Price = 7990, StockQuantity = 8,
                Color = "Красный", ConnectionType = "3.5 мм", WarrantyMonths = 24,
                Description = "Полноразмерные игровые наушники", CategoryId = 3, BrandId = 3, IsActive = true,
                ImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=800"
            });

        modelBuilder.Entity<Review>().HasData(
            new Review { Id = 1, ProductId = 1, UserId = 1, Rating = 5, Comment = "Отличная клавиатура!", CreatedAt = new DateTime(2024, 2, 10), IsApproved = true },
            new Review { Id = 2, ProductId = 2, UserId = 1, Rating = 4, Comment = "Очень удобная мышь", CreatedAt = new DateTime(2024, 2, 15), IsApproved = true });

        modelBuilder.Entity<CartItem>().HasData(
            new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 1 },
            new CartItem { Id = 2, UserId = 1, ProductId = 2, Quantity = 2 });

        base.OnModelCreating(modelBuilder);
    }
}
