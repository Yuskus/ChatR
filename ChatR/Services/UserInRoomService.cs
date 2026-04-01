using ChatR.Models;
using ChatR.Repos;

namespace ChatR.Services;

public class UserInRoomService
{
    private readonly UserInRoomRepo _userInRoomRepo;
    private readonly UserRepo _userRepo;
    private readonly RoomRepo _roomRepo;

    public UserInRoomService(
        UserInRoomRepo userInRoomRepo,
        UserRepo userRepo,
        RoomRepo roomRepo)
    {
        _userInRoomRepo = userInRoomRepo;
        _userRepo = userRepo;
        _roomRepo = roomRepo;
    }

    public async Task AddAsync(int userId, int roomId, RoomRole role = RoomRole.Member)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        var userExists = await _userRepo.GetByIdAsync(userId) != null;
        if (!userExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден.", nameof(userId));

        var roomExists = await _roomRepo.GetByIdAsync(roomId) != null;
        if (!roomExists)
            throw new ArgumentException($"Комната с ID {roomId} не найдена.", nameof(roomId));

        var existing = await _userInRoomRepo.GetByUserAndRoom(userId, roomId);
        if (existing != null)
            throw new ArgumentException($"Пользователь уже состоит в комнате.", nameof(userId));

        await _userInRoomRepo.Add(new UserInRoom
        {
            UserId = userId,
            RoomId = roomId,
            RoomRole = role,
            CreatedAt = DateTime.Now
        });
    }

    public async Task DeleteAsync(int userId, int roomId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        await _userInRoomRepo.Delete(userId, roomId);
    }

    public async Task<List<UserInRoom>> GetByRoomIdAsync(int roomId)
    {
        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        return await _userInRoomRepo.GetByRoomIdAsync(roomId);
    }

    public async Task<UserInRoom?> GetByUserAndRoomAsync(int userId, int roomId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        return await _userInRoomRepo.GetByUserAndRoom(userId, roomId);
    }
}
