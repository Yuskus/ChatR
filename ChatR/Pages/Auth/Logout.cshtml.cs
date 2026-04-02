using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatR.Pages.Auth;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Удаляем куку с токеном
        if (Request.Cookies["auth_token"] != null)
        {
            Response.Cookies.Delete("auth_token", new CookieOptions
            {
                Path = "/",
                Secure = false, // Поставьте true, если используете HTTPS
                SameSite = SameSiteMode.Strict
            });
        }

        // Перенаправляем на вход
        return RedirectToPage("/Auth/Login");
    }
}
