using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Microsoft.IdentityModel.Tokens;


namespace KomunicatorServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(SignInManager<AppUser> signInManager, ILogger<AuthController> logger,
        UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _signInManager = signInManager;
        _logger = logger;
        _userManager = userManager;
        _configuration = configuration;
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

        try
        {
            _logger.LogInformation("Próba weryfikacji hasła dla użytkownika: {Username}", model.Username);


            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning("Próba logowania dla nieistniejącego użytkownika: {Username}", model.Username);
                return Unauthorized(new { Message = "Nieprawidłowa nazwa użytkownika lub hasło" });
            }


            var signInResult = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: true
            );


            if (signInResult.Succeeded)
            {
                _logger.LogInformation("Logowanie zakończone sukcesem dla użytkownika {Username}", model.Username);

                string tokenos = GenerateJwtToken(user);

                return Ok(new LoginSuccessResponse
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Token = tokenos
                });
            }
            else if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("Próba logowania na ZABLOKOWANE konto: {Username}", model.Username);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Message = "Twoje konto jest zablokowane z powodu zbyt wielu prób logowania." });
            }
            else if (signInResult.IsNotAllowed)
            {
                _logger.LogWarning("Próba dostępu do nieaktywowanego konta dla użytkownika {Username}", model.Username);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Message = "Twoje konto nie jest aktywne." });
            }
            else
            {
                _logger.LogWarning("Próba dostępu ze złymi danymi do konta: {Username}", model.Username);
                return Unauthorized(
                    new { Message = "Nieprawidłowa nazwa użytkownika lub hasło." });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Błąd krytyczny podczas logowania użytkownika {Username}", model.Username);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "Wystąpił wewnętrzny błąd serwera. Spróbuj ponownie później." });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
    {
        _logger.LogInformation("Otrzymano żądanie POST na /api/auth/register dla użytkownika {Username}",
            model?.Username ?? "[brak nazwy]");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            AppUser user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email
            };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Rejsctacja zakończona sukcesem dla użytkownika {Username}", model.Username);
                return Ok();
            }
            else
            {
                _logger.LogWarning("Nieudana rejestracja dla użytkownika {Username}. Błędy:", model.Username);
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning($"- {error.Description}");
                }

                return BadRequest(result.Errors);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Wystąpił błąd podczas rejestracji użytkownika {Username}.", model.Username);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "Wystąpił błąd serwera podczas rejestracji." });
        }
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        // claim, czyli to na co chcemy wystawiać token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        
        var claimsIdentity = new ClaimsIdentity(claims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = issuer,
            Audience = audience
        };

        JwtSecurityTokenHandler securityToken = new JwtSecurityTokenHandler();
        
        var token = securityToken.CreateToken(tokenDescriptor);
        var tokenString = securityToken.WriteToken(token);

        return tokenString;
    }
}