using DOANCOSO26.Data;
using DOANCOSO26.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO26.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> CustomerChat()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == user.Id)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.CustomerId = user.Id;
            ViewBag.CustomerName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email;
            return View(messages);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChat(string? customerId)
        {
            var allMessages = await _context.ChatMessages
                .AsNoTracking()
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();
            var userMap = users.ToDictionary(u => u.Id, u => u);

            var threads = allMessages
                .GroupBy(m => m.ConversationId)
                .Select(g =>
                {
                    var last = g.OrderByDescending(m => m.SentAt).First();
                    userMap.TryGetValue(g.Key, out var customer);
                    return new ChatThreadViewModel
                    {
                        CustomerId = g.Key,
                        CustomerName = customer?.FullName ?? customer?.Email ?? "Khách hàng",
                        CustomerEmail = customer?.Email ?? string.Empty,
                        LastMessage = last.MessageText,
                        LastMessageAt = last.SentAt,
                        UnreadCount = g.Count(m => m.SenderRole == Roles.Role_Customer && !m.IsRead)
                    };
                })
                .OrderByDescending(t => t.LastMessageAt)
                .ToList();

            customerId ??= threads.FirstOrDefault()?.CustomerId;

            var selectedMessages = new List<ChatMessage>();
            string? selectedCustomerName = null;

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                selectedMessages = await _context.ChatMessages
                    .Where(m => m.ConversationId == customerId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var unread = selectedMessages
                    .Where(m => m.SenderRole == Roles.Role_Customer && !m.IsRead)
                    .ToList();

                foreach (var message in unread)
                {
                    message.IsRead = true;
                }

                if (unread.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                if (userMap.TryGetValue(customerId, out var selectedCustomer))
                {
                    selectedCustomerName = selectedCustomer.FullName ?? selectedCustomer.Email;
                }
            }

            var model = new ChatAdminViewModel
            {
                SelectedCustomerId = customerId,
                SelectedCustomerName = selectedCustomerName,
                Threads = threads,
                Messages = selectedMessages
            };

            return View(model);
        }
    }
}

