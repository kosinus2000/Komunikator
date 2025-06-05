namespace KomunikatorShared.DTOs
{
    /// <summary>
    /// Reprezentuje model danych odpowiedzi po pomyślnym zalogowaniu użytkownika.
    /// Zawiera informacje o użytkowniku oraz token autoryzacyjny.
    /// </summary>
    public class LoginSuccessResponse
    {
        /// <summary>
        /// Pobiera lub ustawia unikalny identyfikator użytkownika.
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// Pobiera lub ustawia nazwę użytkownika.
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// Pobiera lub ustawia URL do zdjęcia profilowego użytkownika.
        /// </summary>
        public string? ProfilePictureUrl { get; set; }
        /// <summary>
        /// Pobiera lub ustawia token autoryzacyjny JWT.
        /// </summary>
        public string? Token { get; set; }
    }
}