using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.Models;

public class Brand
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Country { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
