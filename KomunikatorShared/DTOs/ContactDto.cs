namespace KomunikatorServer.DTOs
{
    /// <summary>
    /// Reprezentuje uproszczone dane kontaktu użytkownika przekazywane przez API.
    /// </summary>
    public class ContactDto
    {
        /// <summary>
        /// Identyfikator użytkownika.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Nazwa użytkownika (login).
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Adres e-mail użytkownika.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Data rejestracji użytkownika.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Wyświetlana nazwa kontaktu (opcjonalna).
        /// </summary>
        public string? DisplayName { get; set; }
    }
}