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
    UserService userService) : PageModel
{
    private readonly RoomService _roomService = roomService;
    private readonly UserInRoomService _userInRoomService = userInRoomService;
    private readonly UserService _userService = userService;

    public List<Room> UserRooms { get; set; } = [];
    public string CurrentUserEmail { get; set; } = "";
    public int CurrentUserId { get; set; }

    [BindProperty]
    public string NewRoomName { get; set; } = "";

    // Для поиска пользователя
    [BindProperty(SupportsGet = true)]
    public string? UserIdentifier { get; set; }

    public User? FoundUser { get; set; }
    public string? FoundUserError { get; set; }

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

    // 🔥 Поиск и добавление участника
    public async Task<IActionResult> OnPostAddMemberAsync(int roomId, string userIdentifier)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            FoundUserError = "Введите email или ID";
            return await LoadRoomsAsync();
        }

        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var admin = await _userService.GetByEmail(adminEmail!);
        if (admin == null) return Unauthorized();

        var room = await _roomService.GetById(roomId);
        if (room == null)
        {
            TempData[Messages.ERROR] = "Room not found";
            return await LoadRoomsAsync();
        }

        var membership = await _userInRoomService.GetByUserAndRoom(admin.Id, roomId);
        if (membership?.RoomRole != RoomRole.Admin)
        {
            TempData[Messages.ERROR] = "Only the administrator can add members";
            return await LoadRoomsAsync();
        }

        // Поиск пользователя
        User? foundUser;

        if (int.TryParse(userIdentifier, out var userId))
        {
            foundUser = await _userService.GetById(userId);
        }
        else
        {
            foundUser = await _userService.GetByEmail(userIdentifier);
        }

        if (foundUser == null)
        {
            FoundUserError = "Пользователь не найден";
            UserIdentifier = userIdentifier;
            return await LoadRoomsAsync();
        }

        // Проверим, не состоит ли уже
        var existing = await _userInRoomService.GetByUserAndRoom(foundUser.Id, roomId);
        if (existing != null)
        {
            TempData[Messages.ERROR] = "The user is already in the room";
            return await LoadRoomsAsync();
        }

        // Добавляем
        try
        {
            await _userInRoomService.Add(foundUser.Id, roomId, RoomRole.Member);
            TempData[Messages.SUCCESS] = $"User {foundUser.Email} has been added to the room";
        }
        catch (Exception ex)
        {
            TempData[Messages.ERROR] = "Error while adding: " + ex.Message;
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

            var memberships = await _userInRoomService.GetByUserId(user.Id);
            List<Room> rooms = [];
            foreach (var member in memberships)
            {
                var room = await _roomService.GetById(member.RoomId);

                if (room != null)
                {
                    rooms.Add(room);
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