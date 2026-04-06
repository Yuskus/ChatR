using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Repos;

namespace ChatR.Services;

public class RoomService(RoomRepo roomRepo)
{
    private readonly RoomRepo _roomRepo = roomRepo;

    public async Task<Room?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        return await _roomRepo.GetById(id);
    }

    public async Task<Room?> Add(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя комнаты обязательно", nameof(name));

        name = name.Trim();

        if (name.Length == 0)
            throw new ArgumentException("Имя комнаты не может состоять только из пробелов", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Имя комнаты не может превышать 100 символов", nameof(name));

        if (await _roomRepo.ExistsByName(name))
            throw new ArgumentException($"Комната с именем '{name}' уже существует", nameof(name));


        return await _roomRepo.Add(new Room
        {
            Name = name
        });
    }

    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        var room = await _roomRepo.GetById(id)
            ?? throw new ArgumentException($"Комната с ID {id} не найдена", nameof(id));

        await _roomRepo.Delete(room.Id);
    }

    public async Task DeleteInactiveRooms(DateTime olderThan)
    {
        try
        {
            await _roomRepo.DeleteInactiveRoomsBefore(olderThan);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RoomService] Ошибка при удалении комнат: {ex.Message}");
        }
    }

    public async Task Close(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        var room = await _roomRepo.GetById(id)
            ?? throw new ArgumentException($"Комната с ID {id} не найдена", nameof(id));
        if (room.IsClosed)
            return;

        await _roomRepo.Close(id);
    }
}
