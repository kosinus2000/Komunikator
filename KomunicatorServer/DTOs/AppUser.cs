using System.ComponentModel.DataAnnotations;

namespace KomunikatorServer.DTOs
{
    /// <summary>
    /// Reprezentuje użytkownika aplikacji, rozszerzając standardową klasę IdentityUser o dodatkowe właściwości.
    /// </summary>
    public class AppUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
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