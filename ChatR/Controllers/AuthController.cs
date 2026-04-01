using ChatR.Dto.Requests;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(
        AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            bool success = await _authService.RegisterAsync(
                email: request.Email,
                password: request.Password,
                firstName: request.FirstName,
                lastName: request.LastName,
                patronymic: request.Patronymic);

            if (!success)
                return Conflict(new { message = "Пользователь с таким email уже существует" });

            return Ok(new { message = "Регистрация успешна" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = "Ошибка ввода данных", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Внутренняя ошибка сервера", errors = new[] { "Произошла непредвиденная ошибка." } });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);

            if (token == null)
                return Unauthorized(new { message = "Неверный email или пароль" });

            return Ok(token);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Внутренняя ошибка сервера", errors = new[] { "Произошла непредвиденная ошибка." } });
        }
    }
}
