using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class BrandCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Введите название бренда")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Максимум 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "Страна не должна превышать 80 символов")]
    public string? Country { get; set; }
}
