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

    public async Task<int> GetCountByUserId(int userId)
    {
        return await _context.Posts
            .Where(p => p.UserId == userId)
            .CountAsync();
    }

    public async Task<Post?> GetById(int id)
    {
        return await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> Add(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task Update(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Post>> GetFeedPosts(int userId, int skip, int take)
    {
        var followedUserIds = await _context.Observings
            .Where(o => o.UserFromId == userId)
            .Select(o => o.UserToId)
            .ToListAsync();

        return await _context.Posts
            .Where(p => followedUserIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<int> GetFeedPostCount(int userId)
    {
        var followedIds = await _context.Observings
            .Where(o => o.UserFromId == userId)
            .Select(o => o.UserToId)
            .ToListAsync();

        return await _context.Posts
            .Where(p => followedIds.Contains(p.UserId))
            .CountAsync();
    }
}
