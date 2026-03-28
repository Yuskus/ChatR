using ChatR.Data;
using ChatR.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessage(string user, string message)
        {
            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(message))
                return;

            var msg = _dbContext.Messages.Add(new Message
            {
                User = user,
                Content = message
            }).Entity;
            await _dbContext.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", msg.Id, msg.Timestamp, msg.User, msg.Content);
        }

        public async Task DeleteMessage(string messageId)
        {
            if (int.TryParse(messageId, out int id))
            {
                var message = await _dbContext.Messages.FirstOrDefaultAsync(x => x.Id == id);

                if (message != null)
                {
                    _dbContext.Messages.Remove(message);
                    await _dbContext.SaveChangesAsync();

                    await Clients.All.SendAsync("MessageDeleted", messageId);
                }
            }
        }
    }
}
