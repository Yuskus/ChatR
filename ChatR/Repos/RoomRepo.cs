using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class RoomRepo
{
    private readonly ApplicationDbContext _context;

    public RoomRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Rooms
            .AnyAsync(r => r.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Room?> AddAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(room.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await GetByIdAsync(id);
        if (room == null) return;

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task CloseAsync(int id)
    {
        var room = await GetByIdAsync(id);
        if (room != null && !room.IsClosed)
        {
            room.IsClosed = true;
            room.ClosedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
