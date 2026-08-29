using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using AuthConst = ChatR.Models.Constatns.Auth;

namespace ChatR.Pages.Users;

public class ProfileModel(UserService userService, ObservingService observingService, PostService postService) : PageModel
{
    private readonly UserService _userService = userService;
    private readonly ObservingService _observingService = observingService;
    private readonly PostService _postService = postService;
    public int CurrentUserId { get; set; }
    public User? UserToShow { get; set; }
    public bool IsOwnProfile { get; set; }
    public bool IsSubscribed { get; set; }
    public int UsersFromCount { get; set; } = 0;
    public int UsersToCount { get; set; } = 0;
    public List<Post> Posts { get; set; } = [];
    public int TotalPostCount { get; set; } = 0;
    public int TotalPages { get; set; } = 1;
    public int CurrentPage { get; set; } = 1;
    public int PostsPerPage { get; set; } = 10;

    [FromRoute]
    public int Id { get; set; }

    [FromQuery]
    public int Skip { get; set; } = 0;

    [BindProperty]
    public string NewPostContent { get; set; } = "";

    [BindProperty]
    public int EditPostId { get; set; }

    [BindProperty]
    public string EditPostContent { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        var userToShow = await _userService.GetById(Id);
        if (userToShow == null)
            return NotFound();

        UserToShow = userToShow;

        TotalPostCount = await _postService.GetCountByUserId(Id);
        PostsPerPage = 10;
        TotalPages = TotalPostCount > 0
            ? (int)Math.Ceiling((double)TotalPostCount / PostsPerPage)
            : 1;
        CurrentPage = Skip / PostsPerPage + 1;
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        Posts = await _postService.GetLastByUserId(Id, Skip, PostsPerPage);
        
        CurrentUserId = currentUser.Id;
        ViewData["CurrentUserId"] = currentUser.Id;
        
        IsOwnProfile = currentUser.Id == Id;
        UsersFromCount = await _observingService.GetUsersFromCount(Id);
        UsersToCount = await _observingService.GetUsersToCount(Id);

        // Проверяем, подписан ли текущий пользователь на этого пользователя
        if (!IsOwnProfile)
        {
            var observing = await _observingService.GetByIdPair(currentUser.Id, Id);
            IsSubscribed = observing != null;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null || currentUser.Id != Id)
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
            return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id });
        }
    }

    public async Task<IActionResult> OnPostSubscribeAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            if (currentUser.Id == Id)
            {
                TempData[Messages.ERROR] = "Нельзя подписаться на самого себя";
                return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id });
            }

            await _observingService.Add(currentUser.Id, Id);
            TempData[Messages.SUCCESS] = "Вы подписались на пользователя";
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }

        return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id });
    }

    public async Task<IActionResult> OnPostUnsubscribeAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            var observing = await _observingService.GetByIdPair(currentUser.Id, Id);
            if (observing != null)
            {
                await _observingService.Delete(observing.Id);
                TempData[Messages.SUCCESS] = "Вы отписались от пользователя";
            }
            else
            {
                TempData[Messages.ERROR] = "Подписка не найдена";
            }
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }

        return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id });
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            await _postService.Add(currentUser.Id, NewPostContent);
            TempData[Messages.SUCCESS] = "Пост добавлен";
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }

        return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id, skip = Skip });
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            await _postService.Update(EditPostId, currentUser.Id, EditPostContent);
            TempData[Messages.SUCCESS] = "Пост обновлён";
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }

        return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id, skip = Skip });
    }

    public async Task<IActionResult> OnPostDeletePostAsync(int postId)
    {
        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUserEmail))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var currentUser = await _userService.GetByEmail(currentUserEmail);
        if (currentUser == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        try
        {
            await _postService.Delete(postId, currentUser.Id);
            TempData[Messages.SUCCESS] = "Пост удалён";
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }

        return RedirectToPage(Routes.Pages.Users.Profile, new { id = Id, skip = Skip });
    }
}
