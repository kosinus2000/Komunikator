using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KomunikatorServer.Data;
using KomunikatorServer.Models; // Ważne: to zapewnia dostęp do AppUser i ChatMessage
using KomunikatorShared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KomunikatorServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId))
            {
                return Unauthorized(new { message = "Brak identyfikatora użytkownika." });
            }

            var receiver = await _context.Users.FindAsync(messageDto.ReceiverId);
            if (receiver == null)
            {
                Log.Warning("Odbiorca o ID {ReceiverId} nie znaleziony.", messageDto.ReceiverId);
                return NotFound(new { message = "Odbiorca nie znaleziony." });
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            Log.Information("Wiadomość wysłana od {SenderId} do {ReceiverId}. Treść: {Content}", senderId, messageDto.ReceiverId, messageDto.Content);

            var messageResponseDto = new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsMine = true
            };

            return Ok(messageResponseDto);
        }

        [HttpGet("conversation/{contactId}")]
        public async Task<IActionResult> GetConversation(string contactId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Brak identyfikatora użytkownika." });
            }

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == contactId) ||
                            (m.SenderId == contactId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsMine = m.SenderId == currentUserId
                })
                .ToListAsync();

            Log.Information("Pobrano {Count} wiadomości dla konwersacji z {ContactId} dla użytkownika {CurrentUserId}", messages.Count, contactId, currentUserId);
            return Ok(messages);
        }
    }
}