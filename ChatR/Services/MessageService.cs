using ChatR.Models;
using ChatR.Repos;

namespace ChatR.Services;

public class MessageService
{
    private readonly MessageRepo _messageRepo;
    private readonly UserRepo _userRepo;
    private readonly RoomRepo _roomRepo;

    public MessageService(
        MessageRepo messageRepo,
        UserRepo userRepo,
        RoomRepo roomRepo)
    {
        _messageRepo = messageRepo;
        _userRepo = userRepo;
        _roomRepo = roomRepo;
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(id));

        return await _messageRepo.GetByIdAsync(id);
    }

    public async Task AddAsync(string content, int userId, int roomId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Содержание сообщения обязательно", nameof(content));

        content = content.Trim();

        if (content.Length == 0)
            throw new ArgumentException("Сообщение не может состоять только из пробелов", nameof(content));

        if (content.Length > 5000) // Ограничение на длину сообщения
            throw new ArgumentException("Сообщение не может превышать 5000 символов", nameof(content));

        if (userId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        // Проверяем существование пользователя
        var userExists = await _userRepo.GetByIdAsync(userId) != null;
        if (!userExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        // Проверяем существование комнаты
        var roomExists = await _roomRepo.GetByIdAsync(roomId) != null;
        if (!roomExists)
            throw new ArgumentException($"Комната с ID {roomId} не найдена", nameof(roomId));

        await _messageRepo.Add(new Message
        {
            Content = content,
            UserId = userId,
            RoomId = roomId,
            Timestamp = DateTime.Now
        });
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(id));

        var message = await _messageRepo.GetByIdAsync(id);
        if (message == null)
            throw new ArgumentException($"Сообщение с ID {id} не найдено", nameof(id));

        await _messageRepo.Delete(id);
    }

    public async Task<List<Message>> GetListAsync(int roomId, bool ascending = false)
    {
        if (roomId <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(roomId));

        return await _messageRepo.GetList(roomId, ascending);
    }
}
