using ChatR.Helpers;
using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Repos;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ChatR.Services;

public class UserService(UserRepo userRepo)
{
    private readonly UserRepo _userRepo = userRepo;

    public async Task<User?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        return await _userRepo.GetById(id);
    }

    public async Task<User?> GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));

        var trimmedEmail = email.Trim().ToLower();
        if (!Validation.IsValidEmail(trimmedEmail))
            throw new ArgumentException("Некорректный формат email", nameof(email));

        return await _userRepo.GetByEmail(trimmedEmail);
    }

    public async Task<User?> Update(int id, string? password, string firstName, string lastName, string? patronymic)
    {
        // Валидация пароля
        if (!string.IsNullOrWhiteSpace(password) && password.Length < 6)
            throw new ArgumentException("Пароль должен быть не менее 6 символов", nameof(password));

        // Валидация имён
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Имя обязательно", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия обязательна", nameof(lastName));

        patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic.Trim();

        return await _userRepo.Update(
            id,
            password != null ? BCryptNet.HashPassword(password) : null,
            firstName,
            lastName,
            patronymic);
    }

    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        await _userRepo.Delete(id);
    }

    public async Task DeleteInactiveUsers(DateTime olderThan)
    {
        try
        {
            await _userRepo.DeleteInactiveUsersBefore(olderThan);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserService] Ошибка при удалении пользователей: {ex.Message}");
        }
    }
}
