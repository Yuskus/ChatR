using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using AuthConst = ChatR.Models.Constatns.Auth;

namespace ChatR.Pages.Users;

public class EditProfileModel(UserService userService) : PageModel
{
    private readonly UserService _userService = userService;
    public int CurrentUserId { get; set; }
    public int UserId { get; set; }

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
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        UserId = currentUser.Id;
        CurrentUserId = currentUser.Id;
        ViewData["CurrentUserId"] = currentUser.Id;
        
        // Заполняем поля текущими значениями
        FirstName = currentUser.FirstName;
        LastName = currentUser.LastName;
        Patronymic = currentUser.Patronymic;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        Console.WriteLine("OnPostUpdateAsync");
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        if (string.IsNullOrWhiteSpace(FirstName))
            ModelState.AddModelError("FirstName", "Имя обязательно");

        if (string.IsNullOrWhiteSpace(LastName))
            ModelState.AddModelError("LastName", "Фамилия обязательна");

        if (!ModelState.IsValid)
        {
            UserId = currentUser.Id;
            return Page();
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
            {
                TempData[Messages.ERROR] = "Пароль должен быть не менее 6 символов";
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
                TempData[Messages.ERROR] = "Ошибка при обновлении";
                return Page();
            }

            TempData[Messages.SUCCESS] = "Данные успешно обновлены";
            return RedirectToPage(Routes.Pages.Users.Profile, new { id = currentUser.Id });
        }
        catch (Exception ex)
        {
            TempData[Messages.ERROR] = "Ошибка: " + ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        Console.WriteLine("OnPostDeleteAsync");
        var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
        {
            TempData[Messages.ERROR] = "Access denied";
            return Forbid();
        }

        try
        {
            await _userService.Delete(currentUser.Id);

            if (Request.Cookies[AuthConst.TOKEN_COOKIE_NAME] != null)
            {
                Response.Cookies.Delete(AuthConst.TOKEN_COOKIE_NAME);
            }

            return RedirectToPage(Routes.Pages.Auth.Login);
        }
        catch (Exception)
        {
            TempData[Messages.ERROR] = "Не удалось удалить аккаунт";
            return RedirectToPage(Routes.Pages.Index);
        }
    }
}
