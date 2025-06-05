using KomunikatorClient.Core;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.Services
{
    /// <summary>
    /// Klasa serwisu odpowiedzialna za zarządzanie bieżącą sesją użytkownika w aplikacji.
    /// Dziedziczy po <see cref="ObservableObject"/>, aby umożliwiać powiadamianie o zmianach właściwości.
    /// </summary>
    public class CurrentUserSessionService : ObservableObject
    {
        private LoginSuccessResponse? _currentUser;

        /// <summary>
        /// Pobiera lub ustawia bieżącego zalogowanego użytkownika.
        /// Zmiana tej właściwości wywołuje zdarzenia <see cref="ObservableObject.PropertyChanged"/>
        /// dla <see cref="CurrentUser"/> oraz <see cref="IsUserLoggedIn"/>.
        /// </summary>
        public LoginSuccessResponse? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(); // Powiadamia o zmianie właściwości CurrentUser
                OnPropertyChanged(nameof(IsUserLoggedIn)); // Powiadamia o zmianie właściwości IsUserLoggedIn
            }
        }

        /// <summary>
        /// Wskazuje, czy użytkownik jest aktualnie zalogowany.
        /// Zwraca <c>true</c> jeśli <see cref="CurrentUser"/> nie jest null i jego token nie jest pusty.
        /// </summary>
        public bool IsUserLoggedIn => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Token);

        /// <summary>
        /// Ustawia sesję użytkownika na podstawie dostarczonych danych logowania.
        /// </summary>
        /// <param name="user">Obiekt <see cref="LoginSuccessResponse"/> zawierający dane zalogowanego użytkownika.</param>
        public void SetUserSession(LoginSuccessResponse user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Czyści bieżącą sesję użytkownika, efektywnie wylogowując go z aplikacji.
        /// Ustawia <see cref="CurrentUser"/> na null.
        /// </summary>
        public void ClearUserSession()
        {
            CurrentUser = null;
        }
    }
}