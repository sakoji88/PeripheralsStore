using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.Models;

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
    public int Quantity { get; set; } = 1;

    public User? User { get; set; }
    public Product? Product { get; set; }
}
