using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Models.Structure;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ChatR.Pages;

[ValidateAntiForgeryToken]
public class IndexModel(
    RoomService roomService,
    UserInRoomService userInRoomService,
    ObservingService observingService,
    UserService userService) : PageModel
{
    private readonly RoomService _roomService = roomService;
    private readonly UserInRoomService _userInRoomService = userInRoomService;
    private readonly UserService _userService = userService;
    private readonly ObservingService _observingService = observingService;

    public List<Room> UserRooms { get; set; } = [];
    public Dictionary<int, RoomRole> RoomRoles { get; set; } = [];
    public List<User> Following { get; set; } = [];
    public string CurrentUserEmail { get; set; } = "";
    public int CurrentUserId { get; set; }

    [BindProperty]
    public string NewRoomName { get; set; } = "";

    [BindProperty]
    public int SelectedUserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRoomName))
        {
            TempData[Messages.ERROR] = "Room name is required";
            return await LoadRoomsAsync();
        }

        try
        {
            var room = await _roomService.Add(NewRoomName);
            if (room == null)
            {
                TempData[Messages.ERROR] = "Failed to create room";
                return await LoadRoomsAsync();
            }

            TempData[Messages.SUCCESS] = "The room is created";

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userService.GetByEmail(email!);
            if (user != null)
            {
                await _userInRoomService.Add(user.Id, room.Id, RoomRole.Admin);
            }
        }
        catch (ArgumentException ex)
        {
            TempData[Messages.ERROR] = ex.Message;
        }
        catch (Exception)
        {
            TempData[Messages.ERROR] = "Failed to create room";
        }

        return await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnPostJoinAsync(int roomId)
    {
        try
        {
            var email = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userService.GetByEmail(email!);
            if (user == null) return Unauthorized();

            return RedirectToPage(Routes.Pages.Chat.Room, new { roomId });
        }
        catch (Exception)
        {
            TempData[Messages.ERROR] = "Failed to join the room";
            return await LoadRoomsAsync();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int roomId)
    {
        try
        {
            await _roomService.Delete(roomId);
            TempData[Messages.SUCCESS] = "The room has been deleted.";
        }
        catch (Exception)
        {
            TempData[Messages.ERROR] = "Failed to delete room";
        }

        return await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnPostLeaveAsync(int roomId)
    {
        try
        {
            var email = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userService.GetByEmail(email!);
            if (user == null) return Unauthorized();

            await _userInRoomService.Delete(user.Id, roomId);
            TempData[Messages.SUCCESS] = "Вы покинули комнату.";
        }
        catch (Exception)
        {
            TempData[Messages.ERROR] = "Не удалось покинуть комнату";
        }

        return await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnPostAddMemberAsync(int roomId)
    {
        if (SelectedUserId <= 0)
        {
            TempData[Messages.ERROR] = "Выберите пользователя";
            return await LoadRoomsAsync();
        }

        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var admin = await _userService.GetByEmail(adminEmail!);
        if (admin == null) return Unauthorized();

        var room = await _roomService.GetById(roomId);
        if (room == null)
        {
            TempData[Messages.ERROR] = "Комната не найдена";
            return await LoadRoomsAsync();
        }

        var membership = await _userInRoomService.GetByUserAndRoom(admin.Id, roomId);
        if (membership?.RoomRole != RoomRole.Admin)
        {
            TempData[Messages.ERROR] = "Только администратор может добавлять участников";
            return await LoadRoomsAsync();
        }

        var existing = await _userInRoomService.GetByUserAndRoom(SelectedUserId, roomId);
        if (existing != null)
        {
            TempData[Messages.ERROR] = "Этот пользователь уже в комнате";
            return await LoadRoomsAsync();
        }

        try
        {
            await _userInRoomService.Add(SelectedUserId, roomId, RoomRole.Member);
            TempData[Messages.SUCCESS] = "Участник добавлен";
        }
        catch (Exception ex)
        {
            TempData[Messages.ERROR] = "Ошибка при добавлении: " + ex.Message;
        }

        return await LoadRoomsAsync();
    }

    private async Task<IActionResult> LoadRoomsAsync()
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage(Routes.Pages.Auth.Login);

            var user = await _userService.GetByEmail(email);
            if (user == null)
                return RedirectToPage(Routes.Pages.Auth.Login);

            CurrentUserId = user.Id;
            CurrentUserEmail = user.Email;

            ViewData["CurrentUserId"] = user.Id;

            Following = await _observingService.GetMutualObservings(user.Id);

            var memberships = await _userInRoomService.GetByUserId(user.Id);
            List<Room> rooms = [];
            foreach (var member in memberships)
            {
                var room = await _roomService.GetById(member.RoomId);

                if (room != null)
                {
                    rooms.Add(room);
                    RoomRoles[room.Id] = member.RoomRole;
                }
            }

            UserRooms = rooms;
        }
        catch (Exception ex)
        {
            TempData[Messages.ERROR] = "Error loading rooms";
            Console.WriteLine(ex.Message);
        }

        return Page();
    }
}
