using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Укажите корректный email")]
    [StringLength(150, ErrorMessage = "Максимум 150 символов")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
    public string Password { get; set; } = string.Empty;
}
