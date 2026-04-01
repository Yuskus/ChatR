using ChatR.Models;
using ChatR.Repos;

namespace ChatR.Services;

public class UserService
{
    private readonly UserRepo _userRepo;

    public UserService(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(id));

        return await _userRepo.GetByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));

        var trimmedEmail = email.Trim().ToLower();
        if (!IsValidEmail(trimmedEmail))
            throw new ArgumentException("Некорректный формат email", nameof(email));

        return await _userRepo.GetByEmailAsync(trimmedEmail);
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID должно быть положительным числом", nameof(id));

        await _userRepo.DeleteAsync(id);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
