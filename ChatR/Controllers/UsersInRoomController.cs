using ChatR.Dto.Requests;
using ChatR.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersInRoomController : ControllerBase
{
    private readonly UserInRoomService _userInRoomService;

    public UsersInRoomController(
        UserInRoomService userInRoomService)
    {
        _userInRoomService = userInRoomService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> JoinRoom(
        int roomId,
        [FromBody] JoinRoomRequest request)
    {
        try
        {
            await _userInRoomService.Add(request.UserId, roomId, request.Role);

            return Ok(new { message = "Успешно присоединились к чату" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ошибка операции" });
        }
    }

    [HttpDelete("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LeaveRoom(int roomId, int userId)
    {
        try
        {
            await _userInRoomService.Delete(userId, roomId);

            return Ok(new { message = "Вы успешно покинули чат" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = "Ошибка ввода", errors = new[] { ex.Message } });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ошибка при выходе из чата" });
        }
    }
}
