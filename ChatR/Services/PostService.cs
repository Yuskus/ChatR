using ChatR.Models;
using ChatR.Repos;

namespace ChatR.Services;

public class PostService(PostRepo postRepo)
{
    private readonly PostRepo _postRepo = postRepo;

    public async Task<List<Post>> GetLastByUserId(int userId, int skip = 0, int count = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным", nameof(userId));
        if (skip < 0)
            throw new ArgumentException("Количество пролистанных постов не может быть отрицательным", nameof(count));
        if (count <= 0)
            throw new ArgumentException("Количество постов должно быть положительным", nameof(count));

        return await _postRepo.GetLastByUserId(userId, skip, count);
    }
}
