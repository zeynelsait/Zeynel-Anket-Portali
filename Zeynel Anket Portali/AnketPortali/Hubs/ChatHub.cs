using Microsoft.AspNetCore.SignalR;

using System.Security.Claims;
using AnketPortali.Models;

namespace AnketPortali.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        public static HashSet<string> ActiveUsers = new HashSet<string>();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var chatMessage = new ChatMessage
            {
                SenderId = userId,
                SenderName = user,
                Message = message,
                SendTime = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.Id);
        }

        public async Task JoinChat(string user)
        {
            var isAdmin = Context.User?.IsInRole("Admin") ?? false;
            if (!string.IsNullOrEmpty(user))
            {
                ActiveUsers.Add(user);
                
                if (!isAdmin)
                {
                    await Clients.All.SendAsync("UserJoined", user);
                }
                
                await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User?.Identity?.Name;
            var isAdmin = Context.User?.IsInRole("Admin") ?? false;

            if (!string.IsNullOrEmpty(user))
            {
                ActiveUsers.Remove(user);
                
                if (!isAdmin)
                {
                    await Clients.All.SendAsync("UserLeft", user);
                }
                
                await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task DeleteMessage(int messageId)
        {
            if (Context.User?.IsInRole("Admin") ?? false)
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message != null)
                {
                    message.IsDeleted = true;
                    await _context.SaveChangesAsync();
                    await Clients.All.SendAsync("MessageDeleted", messageId, message.Message);
                }
            }
        }
    }
} 