using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class UserInRoomRepo
{
    private readonly ApplicationDbContext _context;

    public UserInRoomRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Add(UserInRoom userInRoom)
    {
        await _context.UsersInRoom.AddAsync(userInRoom);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int userId, int roomId)
    {
        var room = await GetByUserAndRoom(userId, roomId);
        if (room == null) return;

        _context.UsersInRoom.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserInRoom>> GetByRoomIdAsync(int roomId)
    {
        return await _context.UsersInRoom
            .Include(uir => uir.User)
            .Where(uir => uir.RoomId == roomId)
            .OrderBy(uir => uir.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserInRoom?> GetByUserAndRoom(int userId, int roomId)
    {
        return await _context.UsersInRoom
            .Include(uir => uir.User)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoomId == roomId);
    }
}
