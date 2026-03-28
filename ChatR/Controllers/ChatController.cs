using ChatR.Data;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("history")]
        public IActionResult GetMessages()
        {
            var messages = _dbContext.Messages
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Ok(messages);
        }
    }
}
