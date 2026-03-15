using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsBanned { get; set; }

    public DateTime CreatedAt { get; set; }

    public Role? Role { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
