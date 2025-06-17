
using KomunikatorClient.Core;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.MVVM.Model
{
    public class CurrentUserModel : ObservableObject
    {
        private string? _userId;
        private string? _username;
        private string? _profilePictureUrl;
        private string? _token;

        public string? UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public string? Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string? ProfilePictureUrl
        {
            get => _profilePictureUrl;
            set => SetProperty(ref _profilePictureUrl, string.IsNullOrEmpty(value) ? "/Images/icons8-male-user-32.png" : value);
        }

        public string? Token
        {
            get => _token;
            set => SetProperty(ref _token, value);
        }

        public CurrentUserModel()
        {
            _profilePictureUrl = "/Images/icons8-male-user-32.png";
        }

        /// <summary>
        /// Metoda fabryczna do tworzenia CurrentUserModel z LoginSuccessResponse DTO.
        /// Zapewnia, że ProfilePictureUrl zawsze ma wartość (domyślną lub z DTO).
        /// </summary>
        public static CurrentUserModel FromLoginSuccessResponse(LoginSuccessResponse dto)
        {
            if (dto == null)
            {
                return new CurrentUserModel();
            }

            return new CurrentUserModel
            {
                UserId = dto.UserId,
                Username = dto.Username,
                ProfilePictureUrl = string.IsNullOrEmpty(dto.ProfilePictureUrl) ? "KomunikatorClient/Images/icons8-male-user-32.png" : dto.ProfilePictureUrl,
                Token = dto.Token
            };
        }
    }
}