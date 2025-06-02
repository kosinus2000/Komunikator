using KomunikatorClient.Core;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.Services
{
    public class CurrentUserSessionService : ObservableObject
    {
        private LoginSuccessResponse? _currentUser;

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

        public void SetUserSession(LoginSuccessResponse user)
        {
            CurrentUser = user; 
        }

        public void ClearUserSession()
        {
            CurrentUser = null;
        }
    }
}
         