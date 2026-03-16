using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.Models;

public class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(500)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsApproved { get; set; }

    public Product? Product { get; set; }
    public User? User { get; set; }
}
