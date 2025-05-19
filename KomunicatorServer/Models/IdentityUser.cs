using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace KomunikatorServer.Models
{
    public class IdentityUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public DateTime RegistrationDate { get; set; }

        public IdentityUser()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
