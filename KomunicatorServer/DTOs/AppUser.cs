using System.ComponentModel.DataAnnotations;

namespace KomunikatorServer.DTOs
{
    /// <summary>
    /// Reprezentuje użytkownika aplikacji, rozszerzając standardową klasę AppUser o dodatkowe właściwości.
    /// </summary>
    public class AppUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        public ICollection<UserContact> AddedContacts { get; set; } 
        public ICollection<UserContact> IsContactOf { get; set; }  
        public ICollection<ChatMessage> SentMessages { get; set; }
        public ICollection<ChatMessage> ReceivedMessages { get; set; }
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Pobiera lub ustawia datę rejestracji użytkownika.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="AppUser"/>.
        /// Ustawia <see cref="RegistrationDate"/> na aktualny czas UTC w momencie tworzenia obiektu.
        /// </summary>
        public AppUser()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}