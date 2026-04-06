using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class MessageRepo(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Message?> GetById(int id)
    {
        return await _context.Messages
            .Include(m => m.User)
            .Include(m => m.Room)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Message?> Add(Message message)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == message.RoomId);
        if (room == null) return null;

        room.LastMessage = message.Timestamp;

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        return await GetById(message.Id);
    }

    public async Task<Message?> Update(int id, string content)
    {
        var message = await GetById(id);
        if (message == null) return null;

        message.Content = content;
        await _context.SaveChangesAsync();

        return await GetById(id);
    }

    public async Task<Message?> Delete(int id)
    {
        var message = await GetById(id);
        if (message == null) return null;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task DeleteOldMessagesBefore(DateTime olderThan)
    {
        var oldMessages = await _context.Messages
            .Where(x => x.Timestamp < olderThan)
            .ToListAsync();

        _context.RemoveRange(oldMessages);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Message>> GetList(int roomId, bool asc)
    {
        var query = _context.Messages
            .Include(uir => uir.User)
            .Where(uir => uir.RoomId == roomId)
            .AsQueryable();

        query = asc switch
        {
            true => query.OrderBy(uir => uir.Timestamp),
            _ => query.OrderByDescending(uir => uir.Timestamp),
        };

        return await query.ToListAsync();
    }
}
