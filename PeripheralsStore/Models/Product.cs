using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Article { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int StockQuantity { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(50)]
    public string? ConnectionType { get; set; }

    [Range(0, 120)]
    public int WarrantyMonths { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
    public int BrandId { get; set; }

    public bool IsActive { get; set; } = true;

    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
