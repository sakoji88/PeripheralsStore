using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class ReviewCreateViewModel
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [StringLength(500, ErrorMessage = "Отзыв не должен превышать 500 символов")]
    public string Comment { get; set; } = string.Empty;
}
