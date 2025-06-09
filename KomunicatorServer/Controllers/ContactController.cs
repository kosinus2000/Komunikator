using KomunikatorServer.DTOs;
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

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera <see cref="ContactController"/>.
        /// </summary>
        /// <param name="logger">Logger używany do rejestrowania zdarzeń.</param>
        /// <param name="userManager">Menadżer użytkowników do obsługi danych użytkowników.</param>
        public ContactController(ILogger<ContactController> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
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
    }
}
