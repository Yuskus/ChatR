using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using AuthConst = ChatR.Models.Constatns.Auth;

namespace ChatR.Pages.Users;

public class ProfileModel : PageModel
{
    private readonly UserService _userService;

    public ProfileModel(UserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public string FirstName { get; set; } = "";

    [BindProperty]
    public string LastName { get; set; } = "";

    [BindProperty]
    public string? Patronymic { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var user = await _userService.GetByEmail(emailClaim);
        if (user == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        // Заполняем модель
        FirstName = user.FirstName;
        LastName = user.LastName;
        Patronymic = user.Patronymic;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(emailClaim);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        // Валидация входных данных
        if (string.IsNullOrWhiteSpace(FirstName))
            ModelState.AddModelError("FirstName", "Имя обязательно");

        if (string.IsNullOrWhiteSpace(LastName))
            ModelState.AddModelError("LastName", "Фамилия обязательна");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {

            if (!string.IsNullOrWhiteSpace(Password) &&
                Password.Length < 6)
            {
                TempData["ErrorMessage"] = "Incorrect password";
                return Page();
            }

            var updatedUser = await _userService.Update(
                id: currentUser.Id,
                password: Password,
                firstName: FirstName.Trim(),
                lastName: LastName.Trim(),
                patronymic: Patronymic?.Trim());

            if (updatedUser == null)
            {
                TempData["ErrorMessage"] = "Error while updating user";
                return Page();
            }

            TempData["SuccessMessage"] = "Data updated successfully";
            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error while saving: " + ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var user = await _userService.GetByEmail(emailClaim);
        if (user == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            await _userService.Delete(user.Id);

            // Удаляем куку
            if (Request.Cookies[AuthConst.TOKEN_COOKIE_NAME] != null)
            {
                Response.Cookies.Delete(AuthConst.TOKEN_COOKIE_NAME);
            }

            return RedirectToPage(Routes.Pages.Auth.Login);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete account";
            return RedirectToPage();
        }
    }
}
