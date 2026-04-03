using ChatR.Dto.Requests;
using ChatR.Models;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById(int id)
    {
        try
        {
            var room = await _roomService.GetById(id);

            if (room == null)
                return NotFound(new { message = "Комната не найдена" });

            return Ok(room);
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create([FromBody] CreateRoomRequest request)
    {
        try
        {
            await _roomService.Add(request.Name);

            return Ok(new { message = "Комната создана" });
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("уже существует"))
                return Conflict(new { message = ex.Message });

            return BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Не удалось создать комнату" });
        }
    }

    [HttpPut("{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Close(int id)
    {
        try
        {
            await _roomService.Close(id);

            return Ok(new { message = "Комната успешно закрыта" });
        }
        catch (ArgumentException ex)
        {
            return ex.Message.Contains("не найдена")
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ошибка при закрытии комнаты" });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _roomService.Delete(id);

            return Ok(new { message = "Комната удалена" });
        }
        catch (ArgumentException ex)
        {
            return ex.Message.Contains("не найдена")
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ошибка при удалении комнаты" });
        }
    }
}
