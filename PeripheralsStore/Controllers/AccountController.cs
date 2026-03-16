using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PeripheralsStore.Services;
using PeripheralsStore.ViewModels;

namespace PeripheralsStore.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage) = await _authService.RegisterAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        TempData["Success"] = "Регистрация прошла успешно. Теперь войдите в систему.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage, user) = await _authService.ValidateUserAsync(model);
        if (!success || user is null)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        await _authService.SignInAsync(HttpContext, user);
        TempData["Success"] = "Вы успешно вошли в систему";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Вы вышли из аккаунта";
        return RedirectToAction("Index", "Home");
    }
}
