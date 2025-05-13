using KomunikatorServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using IdentityUser = KomunikatorServer.Models.IdentityUser;

namespace KomunicatorServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(SignInManager<IdentityUser> signInManager, ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
    {
        // Operator warunkowy -> jeźeli model jest null, to zwraca null i wywołuje sprawdzenie po ??(jeżeli jest null to zwraca tekst),
        // w przeciwnym wypadku zwraca model
        _logger.LogInformation("Otrzymano żądanie POST na /api/auth/login dla użytkownika {Username}",
            model?.Username ?? "[brak nazwy]");

        // Podstawowe sprawdzenie logowania
        // ModelState jest odpowiedzialny za sprawdzenie informacji o walidacji przesyłąnych danych, wraz z 
        // IsValid zwraca true lub false 
        if (!ModelState.IsValid)
        {
            // --- czyli wywołuje się jeżeli nie ma true czyli w przypadku braku poprawnych danych ---
            //
            _logger.LogWarning("Próba logowania z niepoprawnym modelem (ModelState invalid). Błędy: {@Errors}",
                ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage));

            // zwraca 400 bas request
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Próba weryfikacji hasła dla użytkownika: {Username}", model.Username);
        var signInResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, true);

        if (signInResult.Succeeded)
        {
            _logger.LogInformation("Logowanie zakończone sukcesem dla użytkownika {Username}", model.Username);
            return Ok();
        }
        else if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Próba logowania na ZABLOKOWANE konto: {Username}", model.Username);
            return StatusCode(StatusCodes.Status403Forbidden,
                new { Message = "Twoje konto jest zablokowane z powodu zbyt wielu prób logowania." });
        }else if (signInResult.IsNotAllowed)
        {
            
        }

        return Ok();
    }
}