using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class MessageRepo
{
    private readonly ApplicationDbContext _context;

    public MessageRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        return await _context.Messages
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task Add(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var message = await GetByIdAsync(id);
        if (message == null) return;

        _context.Messages.Remove(message);
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
