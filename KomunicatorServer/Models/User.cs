using System.ComponentModel.DataAnnotations;

namespace KomunikatorServer.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public DateTime RegistrationDate { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
