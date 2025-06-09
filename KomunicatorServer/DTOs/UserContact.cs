using System;

namespace KomunikatorServer.DTOs
{
    /// <summary>
    /// Reprezentuje relację kontaktu pomiędzy dwoma użytkownikami aplikacji.
    /// Przechowuje informację, kto kogo dodał oraz kiedy to nastąpiło.
    /// </summary>
    public class UserContact
    {
        /// <summary>
        /// Identyfikator użytkownika, który dodał kontakt.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Obiekt reprezentujący użytkownika, który dodał kontakt.
        /// </summary>
        public AppUser User { get; set; }

        /// <summary>
        /// Identyfikator użytkownika, który został dodany jako kontakt.
        /// </summary>
        public string ContactId { get; set; }

        /// <summary>
        /// Obiekt reprezentujący użytkownika będącego kontaktem.
        /// </summary>
        public AppUser Contact { get; set; }

        /// <summary>
        /// Data i godzina dodania kontaktu (w czasie UTC).
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Inicjalizuje nową relację kontaktową z aktualną datą i godziną (UTC).
        /// </summary>
        public UserContact()
        {
            AddedDate = DateTime.UtcNow;
        }
    }
}
