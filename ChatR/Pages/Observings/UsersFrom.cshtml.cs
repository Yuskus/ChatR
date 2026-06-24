using System.Security.Claims;
using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatR.Pages.Observings;

public class UsersFromModel(
    UserService userService,
    ObservingService observingService) : PageModel
{
    private readonly UserService _userService = userService;
    private readonly ObservingService _observingService = observingService;

    public IEnumerable<Observing>? Observings { get; set; }
    public int UsersFromCount { get; set; }
    public int CurrentUserId { get; set; }

    [FromRoute]
    public int UserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var user = await _userService.GetByEmail(email);
        if (user == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        CurrentUserId = user.Id;
        ViewData["CurrentUserId"] = user.Id;
            
        var userToShow = await _userService.GetById(UserId);
        if (userToShow == null)
            return NotFound();
        
        try
        {
            Observings = await _observingService.GetUsersFromById(UserId);
            UsersFromCount = await _observingService.GetUsersFromCount(UserId);
        }
        catch (Exception ex)
        {
            TempData[Messages.ERROR] = "Ошибка при загрузке подписок: " + ex.Message;
        }

        return Page();
    }
}
