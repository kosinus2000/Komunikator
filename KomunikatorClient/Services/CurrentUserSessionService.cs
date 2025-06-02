using KomunikatorClient.Core;
using KomunikatorClient.DTOs;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.Services
{
    public class CurrentUserSessionService : ObservableObject
    {
        private LoginSuccessResponse? _currentUser;
        private bool  _isUserLoggedIn;

        public LoginSuccessResponse? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
        }

        public bool IsUserLoggedIn => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Token);


    }
}
         