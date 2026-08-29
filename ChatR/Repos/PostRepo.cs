using ChatR.Data;
using ChatR.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Repos;

public class PostRepo(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Post>> GetLastByUserId(int userId, int skip = 0, int count = 10)
    {
        return await _context.Posts
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(count)
            .ToListAsync();
    }
}
