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
    public class ContactsController : ControllerBase
    {
        private readonly ILogger<ContactsController> _logger;
        private readonly UserManager<AppUser> _userManager;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera <see cref="ContactsController"/>.
        /// </summary>
        /// <param name="logger">Logger używany do logowania zdarzeń.</param>
        /// <param name="userManager">Obiekt zarządzający użytkownikami w systemie.</param>
        public ContactsController(ILogger<ContactsController> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Zwraca listę wszystkich użytkowników jako potencjalnych kontaktów.
        /// </summary>
        /// <returns>Lista użytkowników w postaci <see cref="ContactDto"/>.</returns>
        [Authorize]
        [HttpGet("getContacts")]
        public async Task<ActionResult<List<ContactDto>>> GetContacts()
        {
            _logger.LogInformation("Otrzymano żądanie GET na /api/contacts/getContacts");

            var contacts = await _userManager.Users
                .Select(user => new ContactDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate
                })
                .ToListAsync();

            return contacts;
        }
    }
}
