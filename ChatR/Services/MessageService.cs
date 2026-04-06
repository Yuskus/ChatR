using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Repos;

namespace ChatR.Services;

public class MessageService(
    MessageRepo messageRepo,
    UserRepo userRepo,
    RoomRepo roomRepo)
{
    private readonly MessageRepo _messageRepo = messageRepo;
    private readonly UserRepo _userRepo = userRepo;
    private readonly RoomRepo _roomRepo = roomRepo;

    public async Task<Message?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        return await _messageRepo.GetById(id);
    }

    public async Task<Message?> Add(string content, int userId, int roomId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Содержание сообщения обязательно", nameof(content));

        content = content.Trim();

        if (content.Length == 0)
            throw new ArgumentException("Сообщение не может состоять только из пробелов", nameof(content));

        if (content.Length > 5000) // Ограничение на длину сообщения
            throw new ArgumentException("Сообщение не может превышать 5000 символов", nameof(content));

        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        if (roomId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(roomId));

        // Проверяем существование пользователя
        var userExists = await _userRepo.GetById(userId) != null;
        if (!userExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        // Проверяем существование комнаты
        var roomExists = await _roomRepo.GetById(roomId) != null;
        if (!roomExists)
            throw new ArgumentException($"Комната с ID {roomId} не найдена", nameof(roomId));

        return await _messageRepo.Add(new Message
        {
            Content = content,
            UserId = userId,
            RoomId = roomId,
            Timestamp = DateTime.Now
        });
    }

    public async Task<Message?> Update(int id, string content, int userId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Содержание сообщения обязательно", nameof(content));

        content = content.Trim();

        if (content.Length == 0)
            throw new ArgumentException("Сообщение не может состоять только из пробелов", nameof(content));

        if (content.Length > 5000) // Ограничение на длину сообщения
            throw new ArgumentException("Сообщение не может превышать 5000 символов", nameof(content));

        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        var message = await GetById(id)
            ?? throw new ArgumentException($"Сообщение с ID {id} не найдено", nameof(id));

        if (message.UserId != userId)
            throw new ArgumentException($"Нельзя изменять чужие сообщения", nameof(userId));

        return await _messageRepo.Update(id, content);
    }

    public async Task<Message?> Delete(int id, int userId)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        var message = await _messageRepo.GetById(id)
            ?? throw new ArgumentException($"Сообщение с ID {id} не найдено", nameof(id));

        if (message.UserId != userId)
            throw new ArgumentException($"Нельзя удалять чужие сообщения", nameof(userId));

        return await _messageRepo.Delete(id);
    }

    public async Task DeleteOldMessages(DateTime olderThan)
    {
        try
        {
            await _messageRepo.DeleteOldMessagesBefore(olderThan);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessageService] Ошибка при удалении сообщений: {ex.Message}");
        }
    }

    public async Task<List<Message>> GetList(int roomId, bool ascending = false)
    {
        if (roomId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(roomId));

        return await _messageRepo.GetList(roomId, ascending);
    }
}