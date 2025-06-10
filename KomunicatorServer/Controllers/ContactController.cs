using System.Security.Claims;
using KomunikatorServer.Data;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KomunikatorServer.Controllers
{
    /// <summary>
    /// Kontroler API odpowiedzialny za operacje związane z kontaktami użytkowników.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera <see cref="ContactController"/>.
        /// </summary>
        /// <param name="logger">Logger używany do rejestrowania zdarzeń.</param>
        /// <param name="userManager">Menadżer użytkowników do obsługi danych użytkowników.</param>
        public ContactController(ILogger<ContactController> logger, UserManager<AppUser> userManager,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Zwraca listę wszystkich użytkowników jako potencjalnych kontaktów.
        /// </summary>
        /// <returns>Lista kontaktów w postaci <see cref="ContactDto"/>.</returns>
        [Authorize]
        [HttpGet("getContacts")]
        public async Task<ActionResult<List<ContactDto>>> GetContacts()
        {
            _logger.LogInformation("Otrzymano żądanie GET na /api/contact/getContacts");

            var contacts = await _userManager.Users
                .Select(user => new ContactDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate
                })
                .ToListAsync();

            _logger.LogInformation("Zwracam {Count} kontaktów.", contacts.Count);
            return Ok(contacts);
        }

        [Authorize]
        [HttpPost("addContact")]
        public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
        {
            // === Zaczynamy od bloku try-catch dla całej akcji ===
            try
            {
                _logger.LogInformation("Otrzymano żądanie POST na /api/Contacts/addContact.");

                // 1. Podstawowa walidacja requesta
                if (request == null || (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.UserId)))
                {
                    _logger.LogWarning(
                        "Próba dodania kontaktu z pustymi lub niekompletnymi danymi. Request: {@Request}", request);
                    return BadRequest(new { Message = "Musisz podać nazwę użytkownika lub ID kontaktu do dodania." });
                }

                // Opcjonalna walidacja ModelState.IsValid, jeśli masz atrybuty walidacji na DTO
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Próba dodania kontaktu z niepoprawnym modelem (ModelState invalid). Błędy: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                // 2. Pobierz ID zalogowanego użytkownika (aktualnego użytkownika)
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    _logger.LogError(
                        "Użytkownik nie jest zalogowany, mimo atrybutu [Authorize]. Brak ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd autoryzacji. Użytkownik nie jest zalogowany." });
                }

                // 3. Znajdź użytkownika, którego chcemy dodać jako kontakt
                // Deklarujemy contactUser tutaj, aby był dostępny w całym bloku.
                AppUser contactUser = null; // <--- DODAJ DEKLARACJĘ TUTAJ

                if (!string.IsNullOrEmpty(request.UserId))
                {
                    contactUser = await _userManager.FindByIdAsync(request.UserId);
                }
                else if (!string.IsNullOrEmpty(request.Username))
                {
                    contactUser = await _userManager.FindByNameAsync(request.Username);
                }

                if (contactUser == null)
                {
                    _logger.LogWarning(
                        "Próba dodania nieistniejącego użytkownika jako kontaktu. Dane: {UserId}/{Username}",
                        request.UserId, request.Username);
                    return NotFound(new { Message = "Użytkownik do dodania jako kontakt nie został znaleziony." });
                }

                // 4. Walidacja biznesowa (czy użytkownik nie dodaje siebie, czy już nie jest kontaktem)
                if (contactUser.Id == currentUserId)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać samego siebie do kontaktów.",
                        User.Identity.Name);
                    return BadRequest(new { Message = "Nie możesz dodać samego siebie do kontaktów." });
                }

                // Sprawdź, czy kontakt już istnieje (w tabeli UserContacts)
                bool contactAlreadyExists = await _dbContext.UserContacts
                    .AnyAsync(uc => uc.UserId == currentUserId && uc.ContactId == contactUser.Id);

                if (contactAlreadyExists)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać już istniejący kontakt: {ContactUsername}",
                        User.Identity.Name, contactUser.UserName);
                    return Conflict(new
                        { Message = "Ten użytkownik jest już na Twojej liście kontaktów." }); // 409 Conflict
                }

                // 5. Utwórz nowy kontakt w bazie danych
                var newContactEntry = new UserContact
                {
                    UserId = currentUserId,
                    ContactId = contactUser.Id,
                    AddedDate = DateTime.UtcNow
                };

                _dbContext.UserContacts.Add(newContactEntry);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Użytkownik {Username} pomyślnie dodał kontakt: {ContactUsername}",
                    User.Identity.Name, contactUser.UserName);
                return CreatedAtAction(nameof(GetContacts), new { id = contactUser.Id },
                    new { Message = "Kontakt został pomyślnie dodany." }); // 201 Created
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Wystąpił nieoczekiwany błąd serwera podczas dodawania kontaktu dla użytkownika {Username}.",
                    User.Identity.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Wystąpił wewnętrzny błąd serwera. Spróbuj ponownie później." });
            }
        }
    }
}