using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using KomunikatorServer.Data;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace KomunikatorServer.Controllers
{
    /// <summary>
    /// Kontroler API odpowiedzialny za operacje związane z kontaktami użytkowników.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ILogger<ContactsController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera <see cref="ContactsController"/>.
        /// </summary>
        /// <param name="logger">Logger używany do rejestrowania zdarzeń.</param>
        /// <param name="userManager">Menadżer użytkowników do obsługi danych użytkowników.</param>
        /// <param name="dbContext">Kontekst bazy danych aplikacji.</param>
        public ContactsController(ILogger<ContactsController> logger, UserManager<AppUser> userManager,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Pobiera listę kontaktów powiązanych z aktualnie uwierzytelnionym użytkownikiem.
        /// </summary>
        /// <returns>Zadanie reprezentujące asynchroniczną operację. Wynik zadania zawiera <see cref="ActionResult"/>
        /// opakowujący listę obiektów <see cref="ContactDto"/>, reprezentujących kontakty użytkownika.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("getContacts")]
        public async Task<ActionResult<List<ContactDto>>> GetContacts()
        {
            _logger.LogInformation("Otrzymano żądanie GET na /api/Contacts/getContacts.");
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Autoryzowany użytkownik nie posiada ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd uwierzytelnienia: ID użytkownika nie znaleziono w tokenie." });
                }

                _logger.LogInformation(
                    "Pobieram kontakty dla użytkownika {Username} (ID: {UserId}).",
                    User.Identity.Name, currentUserId);

                var contacts = await _dbContext.UserContacts
                    .Where(uc => uc.UserId == currentUserId)
                    .Include(uc => uc.Contact)
                    .Select(uc => new ContactDto
                    {
                        Id = uc.Contact.Id,
                        Username = uc.Contact.UserName,
                        Email = uc.Contact.Email,
                        RegistrationDate = uc.Contact.RegistrationDate,
                        DisplayName = uc.Contact.DisplayName,
                        ProfilePictureUrl = uc.Contact.ProfilePictureUrl
                    })
                    .ToListAsync();

                _logger.LogInformation("Zwracam {Count} kontaktów dla użytkownika {Username}.", contacts.Count,
                    User.Identity.Name);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd serwera podczas pobierania kontaktów dla użytkownika {Username}.",
                    User.Identity.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Wystąpił wewnętrzny błąd serwera podczas pobierania kontaktów." });
            }
        }

        /// <summary>
        /// Dodaje nowy kontakt dla użytkownika.
        /// </summary>
        /// <param name="request">Obiekt żądania zawierający nazwę użytkownika (TargetUsername) kontaktu do dodania.</param>
        /// <returns>Wynik operacji w postaci odpowiedzi HTTP.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("addContact")]
        public async Task<IActionResult> AddContact([FromBody] AddContactRequestDto request)
        {
            try
            {
                _logger.LogInformation("Otrzymano żądanie POST na /api/Contacts/addContact. TargetUsername: {TargetUsername}", request?.TargetUsername);

                if (request == null || string.IsNullOrWhiteSpace(request.TargetUsername))
                {
                    _logger.LogWarning("Próba dodania kontaktu z pustą lub niekompletną nazwą użytkownika (TargetUsername).");
                    return BadRequest(new { Message = "Musisz podać nazwę użytkownika (TargetUsername) kontaktu do dodania." });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Próba dodania kontaktu z niepoprawnym modelem (ModelState invalid). Błędy: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogError("Użytkownik nie jest zalogowany, mimo atrybutu [Authorize]. Brak ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd autoryzacji. Użytkownik nie jest zalogowany." });
                }

                AppUser contactUser = await _userManager.FindByNameAsync(request.TargetUsername);

                if (contactUser == null)
                {
                    _logger.LogWarning(
                        "Próba dodania nieistniejącego użytkownika jako kontaktu. Nazwa użytkownika: {TargetUsername}",
                        request.TargetUsername);
                    return NotFound(new { Message = "Użytkownik do dodania jako kontakt nie został znaleziony." });
                }

                if (contactUser.Id == currentUserId)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać samego siebie do kontaktów.", User.Identity.Name);
                    return BadRequest(new { Message = "Nie możesz dodać samego siebie do kontaktów." });
                }

                bool contactAlreadyExists = await _dbContext.UserContacts
                    .AnyAsync(uc => uc.UserId == currentUserId && uc.ContactId == contactUser.Id);

                if (contactAlreadyExists)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać już istniejący kontakt: {ContactUsername}",
                        User.Identity.Name, contactUser.UserName);
                    return Conflict(new { Message = "Ten użytkownik jest już na Twojej liście kontaktów." });
                }

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

                var addedContactDto = new ContactDto
                {
                    Id = contactUser.Id,
                    Username = contactUser.UserName,
                    Email = contactUser.Email,
                    RegistrationDate = contactUser.RegistrationDate,
                    DisplayName = contactUser.DisplayName,
                    ProfilePictureUrl = contactUser.ProfilePictureUrl
                };

                return CreatedAtAction(
                    nameof(GetContacts),
                    new { },
                    new { Message = "Kontakt został pomyślnie dodany.", Contact = addedContactDto }
                );
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

        /// <summary>
        /// Wyszukuje użytkowników po fragmencie nazwy użytkownika lub nazwy wyświetlanej.
        /// </summary>
        /// <param name="searchTerm">Ciąg znaków do wyszukania.</param>
        /// <returns>Lista uproszczonych danych użytkowników pasujących do kryteriów wyszukiwania, którzy nie są aktualnie zalogowanym użytkownikiem i nie są już dodani do jego kontaktów.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("searchUsers")]
        public async Task<ActionResult<List<UserSearchDto>>> SearchUsers([FromQuery] string searchTerm)
        {
            _logger.LogInformation("Otrzymano żądanie GET na /api/Contacts/searchUsers. SearchTerm: {SearchTerm}", searchTerm);
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Parametr wyszukiwania jest pusty lub zawiera tylko białe znaki.");
                    return BadRequest(new { Message = "Parametr wyszukiwania nie może być pusty." });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogError("Użytkownik nie jest zalogowany, mimo atrybutu [Authorize]. Brak ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd autoryzacji: ID użytkownika nie znaleziono." });
                }

                var users = await _userManager.Users
                    .Where(u => u.Id != currentUserId &&
                                (u.UserName.Contains(searchTerm) || (u.DisplayName != null && u.DisplayName.Contains(searchTerm))))
                    .Where(u => !_dbContext.UserContacts.Any(uc => uc.UserId == currentUserId && uc.ContactId == u.Id))
                    .Select(u => new UserSearchDto
                    {
                        UserId = u.Id,
                        Username = u.UserName,
                        ProfilePictureUrl = u.ProfilePictureUrl
                    })
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Zwracam {Count} użytkowników dla wyszukiwanej frazy: {SearchTerm}.", users.Count, searchTerm);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił nieoczekiwany błąd serwera podczas wyszukiwania użytkowników dla {SearchTerm}.", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Wystąpił wewnętrzny błąd serwera. Spróbuj ponownie później." });
            }
        }
    }
}