using ChatR.Models;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages.Chat;

public class RoomModel : PageModel
{
    private readonly MessageService _messageService;
    private readonly UserService _userService;
    private readonly RoomService _roomService;
    private readonly UserInRoomService _userInRoomService;

    public int RoomId { get; set; }
    public string RoomName { get; set; } = "";
    public int CurrentUserId { get; set; }
    public string CurrentUserEmail { get; set; } = "";
    public List<Message> Messages { get; set; } = [];

    public RoomModel(
        MessageService messageService,
        UserService userService,
        RoomService roomService,
        UserInRoomService userInRoomService)
    {
        _messageService = messageService;
        _userService = userService;
        _roomService = roomService;
        _userInRoomService = userInRoomService;
    }

    public async Task<IActionResult> OnGetAsync(int roomId)
    {
        RoomId = roomId;

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return RedirectToPage("/Auth/Login");

        var user = await _userService.GetByEmail(email);
        if (user == null)
            return RedirectToPage("/Auth/Login");

        CurrentUserId = user.Id;
        CurrentUserEmail = user.Email;

        // Проверяем, состоит ли пользователь в комнате
        var membership = await _userInRoomService.GetByUserAndRoom(user.Id, roomId);
        if (membership == null)
            return Forbid(); // Не состоит — доступ запрещён

        // Получаем имя комнаты
        var room = await _roomService.GetById(roomId);
        if (room == null)
            return NotFound();

        RoomName = room.Name;

        Messages = await _messageService.GetList(roomId, ascending: true);

        return Page();
    }
}
