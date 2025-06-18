using KomunikatorClient.Core;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.MVVM.Model
{
    /// <summary>
    /// Reprezentuje dane aktualnie zalogowanego użytkownika w aplikacji.
    /// Dziedziczy po ObservableObject, aby wspierać powiadomienia o zmianie właściwości.
    /// </summary>
    public class CurrentUserModel : ObservableObject
    {
        private string? _userId;
        private string? _username;
        private string? _profilePictureUrl;
        private string? _token;

        /// <summary>
        /// Identyfikator użytkownika.
        /// </summary>
        public string? UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        /// <summary>
        /// Nazwa użytkownika.
        /// </summary>
        public string? Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// URL do zdjęcia profilowego użytkownika. Jeśli brak, ustawia domyślny obraz.
        /// </summary>
        public string? ProfilePictureUrl
        {
            get => _profilePictureUrl;
            set => SetProperty(ref _profilePictureUrl, string.IsNullOrEmpty(value) ? "/Images/icons8-male-user-32.png" : value);
        }

        /// <summary>
        /// Token autoryzacyjny użytkownika.
        /// </summary>
        public string? Token
        {
            get => _token;
            set => SetProperty(ref _token, value);
        }

        /// <summary>
        /// Inicjalizuje nową instancję klasy CurrentUserModel.
        /// Ustawia domyślny obraz profilowy.
        /// </summary>
        public CurrentUserModel()
        {
            _profilePictureUrl = "/Images/icons8-male-user-32.png";
        }

        /// <summary>
        /// Metoda fabryczna do tworzenia CurrentUserModel na podstawie obiektu LoginSuccessResponse DTO.
        /// Zapewnia, że ProfilePictureUrl zawsze ma wartość (domyślną lub z DTO).
        /// </summary>
        /// <param name="dto">Obiekt DTO zawierający dane logowania.</param>
        /// <returns>Nowa instancja CurrentUserModel.</returns>
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