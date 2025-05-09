using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace KomunikatorServer.Models
{
    public class User : IdentityUser
    {
        
        
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public DateTime RegistrationDate { get; set; }

        public User()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
