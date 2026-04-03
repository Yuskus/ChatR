using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatR.Pages.Auth;

[ValidateAntiForgeryToken]
public class LoginModel : PageModel
{
    private readonly AuthService _authService;

    public LoginModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public void OnGet()
    {
        // Очистка после выхода
        TempData["ErrorMessage"] = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email и пароль обязательны";
            return Page();
        }

        var token = await _authService.Login(Email, Password);

        if (token == null)
        {
            ErrorMessage = "Неверный email или пароль";
            return Page();
        }

        // Сохраняем токен в куки
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = false,
            Secure = false, // Только HTTPS (в проде)
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(60)
        });

        return RedirectToPage("/Index");
    }
}
