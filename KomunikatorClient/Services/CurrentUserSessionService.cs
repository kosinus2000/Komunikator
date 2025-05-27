using KomunikatorClient.Core;
using KomunikatorClient.DTOs;

namespace KomunikatorClient.Services
{
    public class CurrentUserSessionService : ObservableObject
    {
        private LoginSuccessResponse? _currentUser;
        private bool IsUserLoggedIn;

        public LoginSuccessResponse? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get => IsUserLoggedIn;
            set
            {
                IsUserLoggedIn = value;
                OnPropertyChanged();
            }
        }
    }
}
