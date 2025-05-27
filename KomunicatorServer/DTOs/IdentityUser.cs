using System.ComponentModel.DataAnnotations;

namespace KomunikatorServer.DTOs
{
    public class IdentityUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        public DateTime RegistrationDate { get; set; }

        public IdentityUser()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
