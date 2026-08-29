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

    public async Task<int> GetCountByUserId(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным", nameof(userId));

        return await _postRepo.GetCountByUserId(userId);
    }

    public async Task<Post?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID поста должен быть положительным", nameof(id));

        return await _postRepo.GetById(id);
    }

    public async Task<Post> Add(int userId, string content)
    {
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным", nameof(userId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Текст поста не может быть пустым", nameof(content));
        if (content.Length > 5000)
            throw new ArgumentException("Текст поста не должен превышать 5000 символов", nameof(content));

        var post = new Post
        {
            UserId = userId,
            Content = content.Trim()
        };

        return await _postRepo.Add(post);
    }

    public async Task Update(int id, int userId, string content)
    {
        if (id <= 0)
            throw new ArgumentException("ID поста должен быть положительным", nameof(id));
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным", nameof(userId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Текст поста не может быть пустым", nameof(content));
        if (content.Length > 5000)
            throw new ArgumentException("Текст поста не должен превышать 5000 символов", nameof(content));

        var post = await _postRepo.GetById(id)
            ?? throw new ArgumentException($"Пост с ID {id} не найден", nameof(id));

        if (post.UserId != userId)
            throw new ArgumentException("Редактировать можно только свои посты");

        post.Content = content.Trim();

        await _postRepo.Update(post);
    }

    public async Task Delete(int id, int userId)
    {
        if (id <= 0)
            throw new ArgumentException("ID поста должен быть положительным", nameof(id));
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным", nameof(userId));

        var post = await _postRepo.GetById(id)
            ?? throw new ArgumentException($"Пост с ID {id} не найден", nameof(id));

        if (post.UserId != userId)
            throw new ArgumentException("Удалять можно только свои посты");

        await _postRepo.Delete(id);
    }
}
