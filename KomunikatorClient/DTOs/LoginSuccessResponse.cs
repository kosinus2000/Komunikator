using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomunikatorClient.DTOs
{
    internal class LoginSuccessResponse
    {
        public string? UserId { get; set; }        
        public string? Username { get; set; }      
        public string? ProfilePictureUrl { get; set; } 
        public string? Token { get; set; }        
    }
}
