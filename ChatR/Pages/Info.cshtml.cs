using ChatR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages;

public class InfoModel(UserService userService) : PageModel
{
    private readonly UserService _userService = userService;

    public int CurrentUserId { get; set; }

    public async Task OnGetAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var user = await _userService.GetByEmail(email);
            if (user != null)
            {
                CurrentUserId = user.Id;

                ViewData["CurrentUserId"] = user.Id;
            }
        }
    }
}
