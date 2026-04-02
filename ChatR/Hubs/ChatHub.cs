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
            var message = await _messageService.AddAsync(content, userId, roomId);

            await Clients.Group($"chat_{roomId}").SendAsync("ReceiveMessage",
                message!.Id,
                message.Content,
                message.UserId,
                $"{message.User!.FirstName} {message.User.LastName}",
                message.Timestamp);
        }

        public async Task DeleteMessage(int messageId, int userId)
        {
            var message = await _messageService.GetByIdAsync(messageId);
            if (message?.UserId != userId) return;

            await _messageService.DeleteAsync(messageId);

            await Clients.Group($"chat_{message.RoomId}").SendAsync("MessageDeleted", messageId);
        }

        public async Task UpdateMessage(int messageId, string content, int userId)
        {
            //var message = await _messageService.UpdateAsync(messageId, content, userId);

            //await Clients.Group($"chat_{message.RoomId}").SendAsync("MessageUpdated", messageId, content);
        }
    }
}
