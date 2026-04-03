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

    public async Task Add(int userId, int roomId, RoomRole role = RoomRole.Member)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        var userExists = await _userRepo.GetById(userId) != null;
        if (!userExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден.", nameof(userId));

        var roomExists = await _roomRepo.GetById(roomId) != null;
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

    public async Task Delete(int userId, int roomId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        await _userInRoomRepo.Delete(userId, roomId);
    }

    public async Task<List<UserInRoom>> GetByRoomId(int roomId)
    {
        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        return await _userInRoomRepo.GetByRoomId(roomId);
    }

    public async Task<List<UserInRoom>> GetByUserId(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        return await _userInRoomRepo.GetByUserId(userId);
    }

    public async Task<UserInRoom?> GetByUserAndRoom(int userId, int roomId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        return await _userInRoomRepo.GetByUserAndRoom(userId, roomId);
    }
}
