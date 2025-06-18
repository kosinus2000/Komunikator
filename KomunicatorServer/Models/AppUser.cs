using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KomunikatorServer.DTOs;
using Microsoft.AspNetCore.Identity;

namespace KomunikatorServer.Models
{
    public class AppUser : IdentityUser
    {
        public ICollection<UserContact> AddedContacts { get; set; } = new List<UserContact>();
        public ICollection<UserContact> IsContactOf { get; set; } = new List<UserContact>();
        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime RegistrationDate { get; set; }

        public AppUser()
        {
            RegistrationDate = DateTime.UtcNow;
        }
    }
}