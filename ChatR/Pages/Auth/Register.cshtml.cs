using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatR.Pages.Auth;

[ValidateAntiForgeryToken]
public class RegisterModel(AuthService authService) : PageModel
{
    private readonly AuthService _authService = authService;

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    [BindProperty]
    public string FirstName { get; set; } = "";

    [BindProperty]
    public string LastName { get; set; } = "";

    [BindProperty]
    public string? Patronymic { get; set; }

    public string ErrorMessage { get; set; } = "";

    public void OnGet()
    {
        // Method intentionally left empty.
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "The passwords don't match";
            return Page();
        }

        try
        {
            bool success = await _authService.Register(
                email: Email,
                password: Password,
                firstName: FirstName,
                lastName: LastName,
                patronymic: Patronymic);

            if (!success)
            {
                ErrorMessage = "A user with this email already exists.";
                return Page();
            }

            TempData[Messages.SUCCESS] = "Registration successful. Log in.";
            return RedirectToPage(Routes.Pages.Auth.Login);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while registering: {ex.Message}";
            return Page();
        }
    }
}
