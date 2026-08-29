using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages.Users;

public class AllUsersModel(UserService userService) : PageModel
{
    private readonly UserService _userService = userService;

    public List<User> Users { get; set; } = [];
    public int TotalCount { get; set; } = 0;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int CurrentUserId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        var email = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(email);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        CurrentUserId = currentUser.Id;
        ViewData["CurrentUserId"] = currentUser.Id;

        TotalCount = await _userService.Count();

        if (TotalCount == 0)
        {
            Users = [];
            return Page();
        }

        TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        if (PageNumber < 1) PageNumber = 1;
        if (PageNumber > TotalPages) PageNumber = TotalPages;
        CurrentPage = PageNumber;

        var skip = (PageNumber - 1) * PageSize;

        Users = await _userService.GetAllWithSearch(Search, skip, PageSize);

        return Page();
    }
}
