using ChatR.Models;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(
        UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetById(int id)
    {
        try
        {
            var user = await _userService.GetById(id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = "Некорректный ID", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
