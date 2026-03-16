using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class ProductCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Максимум 120 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите артикул")]
    [StringLength(50, ErrorMessage = "Максимум 50 символов")]
    public string Article { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "Цена должна быть больше 0")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество не может быть отрицательным")]
    public int StockQuantity { get; set; }

    [StringLength(50, ErrorMessage = "Максимум 50 символов")]
    public string? Color { get; set; }

    [StringLength(50, ErrorMessage = "Максимум 50 символов")]
    public string? ConnectionType { get; set; }

    [Range(0, 120, ErrorMessage = "Гарантия должна быть от 0 до 120 месяцев")]
    public int WarrantyMonths { get; set; }

    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }

    [StringLength(300, ErrorMessage = "Ссылка на изображение не должна превышать 300 символов")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Выберите бренд")]
    public int BrandId { get; set; }

    public bool IsActive { get; set; } = true;
}
