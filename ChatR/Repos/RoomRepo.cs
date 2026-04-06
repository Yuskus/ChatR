using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class RoomRepo(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Room?> GetById(int id)
    {
        return await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsByName(string name)
    {
        return await _context.Rooms
            .AnyAsync(r => r.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Room?> Add(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return await GetById(room.Id);
    }

    public async Task Delete(int id)
    {
        var room = await GetById(id);
        if (room == null) return;

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInactiveRoomsBefore(DateTime olderThan)
    {
        var oldRooms = await _context.Rooms
            .Where(x =>
                (x.LastMessage == null && x.CreatedAt < olderThan) ||
                x.LastMessage < olderThan)
            .ToListAsync();

        _context.Rooms.RemoveRange(oldRooms);
        await _context.SaveChangesAsync();
    }

    public async Task Close(int id)
    {
        var room = await GetById(id);
        if (room != null && !room.IsClosed)
        {
            room.IsClosed = true;
            room.ClosedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
