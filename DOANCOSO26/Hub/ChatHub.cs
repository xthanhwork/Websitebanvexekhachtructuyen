using System.Security.Claims;
using DOANCOSO26.Data;
using DOANCOSO26.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO26.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const string AdminGroup = "SupportAdmins";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            }

            if (Context.User?.IsInRole(Roles.Role_Admin) == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            }

            await base.OnConnectedAsync();
        }

        public async Task SendCustomerMessage(string message)
        {
            var sender = await GetCurrentUserAsync();
            if (sender == null)
            {
                throw new HubException("Bạn cần đăng nhập để gửi tin nhắn.");
            }

            message = NormalizeMessage(message);
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var chatMessage = new ChatMessage
            {
                ConversationId = sender.Id,
                SenderId = sender.Id,
                SenderName = DisplayName(sender),
                SenderRole = Roles.Role_Customer,
                MessageText = message,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var payload = ToPayload(chatMessage);
            await Clients.Group(UserGroup(sender.Id)).SendAsync("ReceiveCustomerMessage", payload);
            await Clients.Group(AdminGroup).SendAsync("ReceiveAdminMessage", payload);
        }

        [Authorize(Roles = "Admin")]
        public async Task SendAdminMessage(string customerId, string message)
        {
            var sender = await GetCurrentUserAsync();
            if (sender == null)
            {
                throw new HubException("Không xác định được tài khoản admin.");
            }

            message = NormalizeMessage(message);
            if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                throw new HubException("Không tìm thấy khách hàng cần trả lời.");
            }

            var chatMessage = new ChatMessage
            {
                ConversationId = customer.Id,
                SenderId = sender.Id,
                SenderName = DisplayName(sender),
                SenderRole = Roles.Role_Admin,
                MessageText = message,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var payload = ToPayload(chatMessage);
            await Clients.Group(UserGroup(customer.Id)).SendAsync("ReceiveCustomerMessage", payload);
            await Clients.Group(AdminGroup).SendAsync("ReceiveAdminMessage", payload);
        }

        [Authorize(Roles = "Admin")]
        public async Task MarkConversationRead(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return;
            }

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ConversationId == customerId && m.SenderRole == Roles.Role_Customer && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return Context.User == null ? null : await _userManager.GetUserAsync(Context.User);
        }

        private static string UserGroup(string userId) => $"User_{userId}";

        private static string DisplayName(ApplicationUser user)
        {
            return !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email ?? user.UserName ?? "User";
        }

        private static string NormalizeMessage(string message)
        {
            return (message ?? string.Empty).Trim();
        }

        private static object ToPayload(ChatMessage message)
        {
            return new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                senderName = message.SenderName,
                senderRole = message.SenderRole,
                messageText = message.MessageText,
                sentAt = message.SentAt.ToString("dd/MM/yyyy HH:mm")
            };
        }
    }
}
