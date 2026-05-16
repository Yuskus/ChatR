using ChatR.Models;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages.Chat;

public class RoomModel(
    MessageService messageService,
    UserService userService,
    RoomService roomService,
    UserInRoomService userInRoomService) : PageModel
{
    private readonly MessageService _messageService = messageService;
    private readonly UserService _userService = userService;
    private readonly RoomService _roomService = roomService;
    private readonly UserInRoomService _userInRoomService = userInRoomService;

    public int RoomId { get; set; }
    public string RoomName { get; set; } = "";
    public int CurrentUserId { get; set; }
    public string CurrentUserEmail { get; set; } = "";
    public List<Message> Messages { get; set; } = [];
    public List<User> RoomUsers { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int roomId)
    {
        RoomId = roomId;

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return RedirectToPage(Routes.Pages.Auth.Login);

        var user = await _userService.GetByEmail(email);
        if (user == null)
            return RedirectToPage(Routes.Pages.Auth.Login);

        CurrentUserId = user.Id;
        CurrentUserEmail = user.Email;

        ViewData["CurrentUserId"] = user.Id;

        var membership = await _userInRoomService.GetByUserAndRoom(user.Id, roomId);
        if (membership == null)
            return Forbid();

        var room = await _roomService.GetById(roomId);
        if (room == null)
            return NotFound();

        RoomName = room.Name;

        Messages = await _messageService.GetList(roomId, ascending: true);

        var usersInRoom = await _userInRoomService.GetByRoomId(roomId);
        if (usersInRoom == null)
            return NotFound();

        RoomUsers = usersInRoom
            .Where(x => x.User != null)
            .Select(x => x.User)
            .Cast<User>()
            .ToList();

        return Page();
    }
}
