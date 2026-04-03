using ChatR.Models;
using ChatR.Repos;
using Microsoft.EntityFrameworkCore;
using ChatR.Models.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ChatR.Services;

public class AuthService
{
    private readonly UserRepo _userRepo;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserRepo userRepo)
    {
        _userRepo = userRepo;
        _jwtSettings = new JwtSettings();
    }

    public async Task<bool> Register(
        string email,
        string password,
        string firstName,
        string lastName,
        string? patronymic = null)
    {
        // Валидация email
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email обязателен", nameof(email));
        email = email.Trim().ToLower();
        if (!IsValidEmail(email))
            throw new ArgumentException("Некорректный формат email", nameof(email));

        // Валидация пароля
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль обязателен", nameof(password));
        if (password.Length < 6)
            throw new ArgumentException("Пароль должен быть не менее 6 символов", nameof(password));

        // Валидация имён
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Имя обязательно", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия обязательна", nameof(lastName));

        patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic.Trim();

        // Проверка на дубликат
        if (await _userRepo.ExistsByEmail(email))
            return false;

        try
        {
            await _userRepo.Add(new User
            {
                Email = email,
                Password = BCryptNet.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                CreatedAt = DateTime.Now,
                Role = UserRole.User
            });

            return true;
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Неожиданная ошибка при добавлении пользователя: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _userRepo.GetByEmail(email.Trim().ToLower());

        if (user == null || !BCryptNet.Verify(password, user.Password))
            return null;

        string? token = GenerateJwtToken(user);

        if (token == null)
            return null;

        await _userRepo.Login(user.Id);

        return token;
    }

    private string? GenerateJwtToken(User user)
    {
        if (_jwtSettings.Secret == null) return null;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ],
            expires: DateTime.Now.AddMinutes(_jwtSettings.TokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
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