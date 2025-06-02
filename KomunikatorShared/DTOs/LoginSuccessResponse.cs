namespace KomunikatorShared.DTOs
{
    public class LoginSuccessResponse
    {
        public string? UserId { get; set; }        
        public string? Username { get; set; }      
        public string? ProfilePictureUrl { get; set; } 
        public string? Token { get; set; }        
    }
}
