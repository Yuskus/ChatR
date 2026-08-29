using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages;

public class FeedModel(PostService postService, UserService userService) : PageModel
{
    private readonly PostService _postService = postService;
    private readonly UserService _userService = userService;

    public List<Post> Posts { get; set; } = [];
    public int TotalCount { get; set; } = 0;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int CurrentUserId { get; set; }

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

        TotalCount = await _postService.GetFeedPostCount(currentUser.Id);

        if (TotalCount == 0)
        {
            Posts = [];
            return Page();
        }

        TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        if (PageNumber < 1) PageNumber = 1;
        if (PageNumber > TotalPages) PageNumber = TotalPages;
        CurrentPage = PageNumber;

        var skip = (PageNumber - 1) * PageSize;

        Posts = await _postService.GetFeedPosts(currentUser.Id, skip, PageSize);

        return Page();
    }
}
