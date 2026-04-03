using ChatR.Models.Structure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthConst = ChatR.Models.Constatns.Auth;

namespace ChatR.Pages.Auth;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Удаляем куку с токеном
        if (Request.Cookies[AuthConst.TOKEN_COOKIE_NAME] != null)
        {
            Response.Cookies.Delete(AuthConst.TOKEN_COOKIE_NAME, new CookieOptions
            {
                Path = "/",
                Secure = false, // Поставьте true, если используете HTTPS
                SameSite = SameSiteMode.Strict
            });
        }

        // Перенаправляем на вход
        return RedirectToPage(Routes.Pages.Auth.Login);
    }
}
