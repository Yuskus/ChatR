using ChatR.Models;
using ChatR.Repos;

namespace ChatR.Services;

public class RoomService
{
    private readonly RoomRepo _roomRepo;

    public RoomService(RoomRepo roomRepo)
    {
        _roomRepo = roomRepo;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID комнаты должно быть положительным числом", nameof(id));

        return await _roomRepo.GetByIdAsync(id);
    }

    public async Task<Room?> AddAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя комнаты обязательно", nameof(name));

        name = name.Trim();

        if (name.Length == 0)
            throw new ArgumentException("Имя комнаты не может состоять только из пробелов", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Имя комнаты не может превышать 100 символов", nameof(name));

        if (await _roomRepo.ExistsByNameAsync(name))
            throw new ArgumentException($"Комната с именем '{name}' уже существует", nameof(name));


        return await _roomRepo.AddAsync(new Room
        {
            Name = name
        });
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID комнаты должно быть положительным числом", nameof(id));

        var room = await _roomRepo.GetByIdAsync(id);
        if (room == null)
            throw new ArgumentException($"Комната с ID {id} не найдена", nameof(id));

        await _roomRepo.DeleteAsync(id);
    }

    public async Task CloseAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID комнаты должно быть положительным числом", nameof(id));

        var room = await _roomRepo.GetByIdAsync(id);
        if (room == null)
            throw new ArgumentException($"Комната с ID {id} не найдена", nameof(id));

        if (room.IsClosed)
            return;

        await _roomRepo.CloseAsync(id);
    }
}
