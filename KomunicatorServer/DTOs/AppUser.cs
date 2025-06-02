using System.ComponentModel.DataAnnotations;

namespace KomunikatorServer.DTOs
{
    public class AppUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        public DateTime RegistrationDate { get; set; }

        public AppUser()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
