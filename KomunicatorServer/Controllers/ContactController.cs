using System.Security.Claims;
using KomunikatorServer.Data;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
        /// Pobiera listę kontaktów powiązanych z aktualnie uwierzytelnionym użytkownikiem.
        /// </summary>
        /// <returns>Zadanie reprezentujące asynchroniczną operację. Wynik zadania zawiera <see cref="ActionResult"/>
        /// opakowujący listę obiektów <see cref="ContactDto"/>, reprezentujących kontakty użytkownika.</returns>
       [Authorize]
        [HttpGet("getContacts")]
        public async Task<ActionResult<List<ContactDto>>> GetContacts()
        {
            _logger.LogInformation("Otrzymano żądanie GET na /api/contact/getContacts");

            try
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    _logger.LogWarning(
                        "Użytkownik nie jest zalogowany, mimo atrybutu [Authorize]. Brak ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd autoryzacji. Użytkownik nie jest zalogowany." });
                }

                var contacts = await _dbContext.UserContacts
                    .Where(uc => uc.UserId == currentUserId)
                    .Include(uc => uc.Contact)
                    .Select(uc => new ContactDto
                    {
                        Id = uc.Contact.Id,
                        Username = uc.Contact.UserName,
                        Email = uc.Contact.Email,
                        ProfilePictureUrl = uc.Contact.ProfilePictureUrl ?? "KomunicatorServer/wwwroot/ProfilePictures/icons8-male-user-32.png",
                        RegistrationDate = uc.Contact.RegistrationDate,
                        DisplayName = uc.Contact.DisplayName
                    }).ToListAsync();
                _logger.LogInformation("Zwracam {Count} kontaktów dla użytkownika {Username}.", contacts.Count,
                    User.Identity.Name);
                return Ok(contacts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Wystąpił błąd serwera podczas pobierania kontaktów dla użytkownika {Username}.",
                    User.Identity.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Wystąpił błąd serwera podczas pobierania kontaktów." });
            }
        }

        /// <summary>
        /// Dodaje nowy kontakt dla użytkownika.
        /// </summary>
        /// <param name="request">Obiekt żądania zawierający dane dotyczące kontaktu do dodania.</param>
        /// <returns>Wynik operacji w postaci odpowiedzi HTTP. W przypadku powodzenia metoda zwraca kod HTTP 201 (Created). W przypadku błędów walidacji lub serwera metoda zwraca odpowiedni kod błędu.</returns>
        [Authorize]
        [HttpPost("addContact")]
        public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
        {
            try
            {
                _logger.LogInformation("Otrzymano żądanie POST na /api/Contacts/addContact.");

                //Podstawowa walidacja requesta
                if (request == null || (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.UserId)))
                {
                    _logger.LogWarning(
                        "Próba dodania kontaktu z pustymi lub niekompletnymi danymi. Request: {@Request}", request);
                    return BadRequest(new { Message = "Musisz podać nazwę użytkownika lub ID kontaktu do dodania." });
                }

                //walidacja, na wypadek braku [Authenticate]
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Próba dodania kontaktu z niepoprawnym modelem (ModelState invalid). Błędy: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                //Pobranie id usera
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    _logger.LogError(
                        "Użytkownik nie jest zalogowany, mimo atrybutu [Authorize]. Brak ClaimTypes.NameIdentifier.");
                    return Unauthorized(new { Message = "Błąd autoryzacji. Użytkownik nie jest zalogowany." });
                }

                //wyszukanie uzytkownika który ma być dodoany jako kontakt
                AppUser contactUser = null;

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

                //walidacja, czy użytkownik nie dodaje sam siebie jako kontaktu
                if (contactUser.Id == currentUserId)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać samego siebie do kontaktów.",
                        User.Identity.Name);
                    return BadRequest(new { Message = "Nie możesz dodać samego siebie do kontaktów." });
                }

                //sprawdzenie czy użytkownik już istnieje w bazie danych jako kontakt
                bool contactAlreadyExists = await _dbContext.UserContacts
                    .AnyAsync(uc => uc.UserId == currentUserId && uc.ContactId == contactUser.Id);

                if (contactAlreadyExists)
                {
                    _logger.LogWarning("Użytkownik {Username} próbował dodać już istniejący kontakt: {ContactUsername}",
                        User.Identity.Name, contactUser.UserName);
                    return Conflict(new
                        { Message = "Ten użytkownik jest już na Twojej liście kontaktów." }); // 409 Conflict
                }

                // utworzenie nowego kontaktu w bazie danych
                var newContactEntry = new UserContact
                {
                    UserId = currentUserId,
                    ContactId = contactUser.Id,
                    AddedDate = DateTime.UtcNow
                };
                //dodanie nowego kontaktu do bazy danych
                _dbContext.UserContacts.Add(newContactEntry);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Użytkownik {Username} pomyślnie dodał kontakt: {ContactUsername}",
                    User.Identity.Name, contactUser.UserName);
                return CreatedAtAction(nameof(GetContacts), new { id = contactUser.Id },
                    new { Message = "Kontakt został pomyślnie dodany." });
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