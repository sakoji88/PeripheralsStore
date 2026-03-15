using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class CategoryCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Введите название категории")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Максимум 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Описание не должно превышать 300 символов")]
    public string? Description { get; set; }
}
