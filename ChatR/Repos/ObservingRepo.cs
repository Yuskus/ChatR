using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class ObservingRepo(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Observing?> Add(Observing observing)
    {
        await _context.Observings.AddAsync(observing);
        await _context.SaveChangesAsync();

        return await GetById(observing.Id);
    }

    public async Task<Observing?> GetById(int id)
    {
        return await _context.Observings
            .Include(o => o.UserFrom)
            .Include(o => o.UserTo)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task Delete(int id)
    {
        var observing = await _context.Observings
            .FirstOrDefaultAsync(o => o.Id == id);
        if (observing == null) return;

        _context.Observings.Remove(observing);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Observing>> GetUsersFromById(int userId)
    {
        return await _context.Observings
            .Where(o => o.UserFromId == userId)
            .Include(o => o.UserTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Observing>> GetUsersToById(int userId)
    {
        return await _context.Observings
            .Where(o => o.UserToId == userId)
            .Include(o => o.UserFrom)
            .ToListAsync();
    }

    public async Task<int> GetUsersFromCount(int userId)
    {
        return await _context.Observings.CountAsync(o => o.UserFromId == userId);
    }

    public async Task<int> GetUsersToCount(int userId)
    {
        return await _context.Observings.CountAsync(o => o.UserToId == userId);
    }

    public async Task<Observing?> GetByIdPair(int userFromId, int userToId)
    {
        return await _context.Observings
            .FirstOrDefaultAsync(x => x.UserFromId == userFromId && x.UserToId == userToId);
    }

    public async Task<IEnumerable<User>> GetMutualObservings(int userId)
    {
        var mutualUserIds = _context.Observings
            .Where(x => x.UserToId == userId || x.UserFromId == userId)
            .Select(x => x.UserToId == userId ? x.UserFromId : x.UserToId)
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        return await _context.Users
            .Where(x => mutualUserIds.Contains(x.Id))
            .ToListAsync();
    }
}