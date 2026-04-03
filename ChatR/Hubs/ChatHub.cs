using ChatR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;

        public ChatHub(
            MessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(string content, int userId, int roomId)
        {
            Console.WriteLine("1 - " + Context.User?.Identity?.IsAuthenticated.ToString());
            var message = await _messageService.Add(content, userId, roomId);

            if (message != null)
            {
                var fullname = message.User != null
                    ? $"{message.User.FirstName} {message.User.LastName}"
                    : "Unknown Unknown";

                await Clients.Group($"chat_{roomId}").SendAsync("ReceiveMessage",
                    message.Id,
                    message.Content,
                    message.UserId,
                    fullname,
                    message.Timestamp);
            }
        }

        public async Task DeleteMessage(int messageId, int userId)
        {
            var message = await _messageService.Delete(messageId, userId);

            if (message != null)
            {
                await Clients.Group($"chat_{message.RoomId}").SendAsync("MessageDeleted", messageId);
            }
        }

        public async Task UpdateMessage(int messageId, string content, int userId)
        {
            var message = await _messageService.Update(messageId, content, userId);

            if (message != null)
            {
                await Clients.Group($"chat_{message.RoomId}").SendAsync("MessageUpdated", messageId, content);
            }
        }
    }
}
