using System.ComponentModel.DataAnnotations;

namespace PeripheralsStore.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Полное имя должно быть от 2 до 100 символов")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Укажите корректный email")]
    [StringLength(150, ErrorMessage = "Максимум 150 символов")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
