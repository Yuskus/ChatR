using ChatR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatR.Pages
{
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnGet()
        {
        }

        public IActionResult OnGetLoadMessages()
        {
            var messages = _dbContext.Messages
                .OrderBy(m => m.Timestamp)
                .ToList();

            return new JsonResult(messages);
        }
    }
}
