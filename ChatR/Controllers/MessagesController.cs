using ChatR.Dto.Requests;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MessagesController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessagesController(MessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Add(
        int roomId,
        [FromBody] CreateMessageRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Тело запроса не может быть пустым" });

            await _messageService.AddAsync(request.Content, request.UserId, roomId);

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return ex.Message.Contains("не найден")
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Не удалось отправить сообщение" });
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetList(
        int roomId,
        [FromQuery] bool ascending = false)
    {
        try
        {
            var messages = await _messageService.GetListAsync(roomId, ascending);

            return Ok(messages);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ошибка при получении сообщений" });
        }
    }
}

