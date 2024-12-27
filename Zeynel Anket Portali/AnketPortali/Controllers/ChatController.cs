using AnketPortali.Hubs;
using AnketPortali.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AnketPortali.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SendTime)
                .Take(100)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(id);
                if (message != null)
                {
                    message.IsDeleted = true;
                    await _context.SaveChangesAsync();

                    // SignalR ile tüm kullanıcılara bildir
                    await _hubContext.Clients.All.SendAsync("MessageDeleted", id, message.Message);

                    return Json(new { success = true });
                }
                return Json(new { success = false, error = "Mesaj bulunamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveUsers()
        {
            var activeUsers = ChatHub.ActiveUsers;
            return Json(activeUsers);
        }
    }
}
