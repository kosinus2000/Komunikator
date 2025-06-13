using System.Security.Claims;
using KomunikatorServer.Data;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KomunikatorServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public MessagesController(ILogger<MessagesController> logger, UserManager<AppUser> userManager,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        _logger.LogInformation("Otrzymano żądanie POST na /api/Messages/send od użytkownika.");
        string? senderId = null;
        try
        {
            senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //walidowanie danych wejsciowych
            if (request == null || string.IsNullOrEmpty(request.ReceiverId) ||
                string.IsNullOrWhiteSpace(request.Content)) // <-- ZMIANA TUTAJ: Sprawdza Content
            {
                _logger.LogWarning("Próba wysłania wiadomości z pustym odbiorcą lub treścią. Request: {@Request}",
                    request);
                // <-- ZMIANA TUTAJ: Poprawiona składnia anonimowego obiektu
                return BadRequest(new { Message = "Musisz podać odbiorcę i treść wiadomości." });
            }

            //pobranie ID zalogowanego użytkowniaka
            if (senderId == null)
            {
                _logger.LogError("Zalogowany użytkownik nie posiada ID (SenderId jest null) mimo [Authorize].");
                return Unauthorized(new { Message = "Błąd autoryzacji. ID nadawcy nie mogło zostać pobrane." });
            }

            // wyszukanie odbiorcy
            var receiverUser = await _userManager.FindByIdAsync(request.ReceiverId);
            if (receiverUser == null)
            {
                _logger.LogWarning("Próba wysłania wiadomości do nieistniejącego odbiorcy o ID: {ReceiverId}",
                    request.ReceiverId);
                return NotFound(new { Message = "Odbiorca wiadomości nie został znaleziony." });
            }

            // obiekt wiadomosci chatmessage
            var chatMessage =
                new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = request.ReceiverId,
                    Content = request.Content,
                    Timestamp = DateTime.UtcNow
                };

            // zapis wiadomosci do bazy danych

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Wiadomość od {SenderId} do {ReceiverId} pomyślnie zapisana. Treść: {ContentPreview}",
                senderId, request.ReceiverId,
                request.Content.Length > 50 ? request.Content.Substring(0, 50) + "..." : request.Content);

            // zwrot suuuukcesu uauuauauaua
            return StatusCode(StatusCodes.Status201Created, new { Message = "Wiadomość została pomyślnie wysłana." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd serwera podczas wysyłania wiadomości od {SenderId} do {ReceiverId}.",
                senderId, request.ReceiverId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "Wystąpił wewnętrzny błąd serwera podczas wysyłania wiadomości." });
        }
    }
}