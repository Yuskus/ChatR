using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class UserRepo(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<int> Count()
    {
        return await _context.Users
            .CountAsync();
    }

    public async Task<List<User>> GetAll()
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<User>> GetAllWithSearch(string? search, int skip, int take)
    {
        IQueryable<User> query = _context.Users.OrderByDescending(u => u.CreatedAt);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var terms = search.Trim().ToLower().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(u =>
                terms.All(t =>
                    u.LastName.ToLower().Contains(t) ||
                    u.FirstName.ToLower().Contains(t) ||
                    (u.Patronymic != null && u.Patronymic.ToLower().Contains(t))));
        }

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<User?> GetById(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.Trim().ToLower());
    }

    public async Task Add(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task Login(int id)
    {
        var user = await GetById(id);
        if (user == null) return;

        user.LastLogin = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<User?> Update(int id, string? passwordHash, string firstName, string lastName, string? patronymic)
    {
        var user = await GetById(id);
        if (user == null) return null;

        if (passwordHash != null)
            user.Password = passwordHash;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Patronymic = patronymic;
        await _context.SaveChangesAsync();

        return await GetById(id);
    }

    public async Task Delete(int id)
    {
        var user = await GetById(id);
        if (user == null) return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInactiveUsersBefore(DateTime olderThan)
    {
        var oldUsers = await _context.Users
            .Where(x =>
                (x.LastLogin == null && x.CreatedAt < olderThan) ||
                x.LastLogin < olderThan)
            .ToListAsync();

        _context.Users.RemoveRange(oldUsers);
        await _context.SaveChangesAsync();
    }
}
