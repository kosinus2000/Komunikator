namespace KomunikatorShared.DTOs;

/// <summary>
/// Reprezentuje model danych używany do żądania rejestracji nowego użytkownika.
/// Zawiera nazwę użytkownika, hasło oraz adres e-mail wymagane do utworzenia nowego konta.
/// </summary>
public class RegisterRequestModel
{
    /// <summary>
    /// Pobiera lub ustawia nazwę użytkownika dla nowego konta.
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// Pobiera lub ustawia hasło dla nowego konta.
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// Pobiera lub ustawia adres e-mail dla nowego konta.
    /// </summary>
    public string Email { get; set; }
}