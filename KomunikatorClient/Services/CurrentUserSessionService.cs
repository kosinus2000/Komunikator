using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorShared.DTOs;
using System.ComponentModel;

namespace KomunikatorClient.Services
{
    /// <summary>
    /// Serwis zarządzający sesją aktualnie zalogowanego użytkownika.
    /// Przechowuje dane użytkownika i informuje o zmianach stanu logowania.
    /// </summary>
    public class CurrentUserSessionService : ObservableObject
    {
        private CurrentUserModel? _currentUser;

        /// <summary>
        /// Pobiera lub ustawia model bieżącego użytkownika.
        /// Przy zmianie subskrybuje lub odsubskrybuje zdarzenia PropertyChanged
        /// i aktualizuje stan zalogowania użytkownika.
        /// </summary>
        public CurrentUserModel? CurrentUser
        {
            get => _currentUser;
            set
            {
                // Odsubskrybuj zdarzenia od poprzedniego obiektu, jeśli istnieje
                if (_currentUser != null) _currentUser.PropertyChanged -= CurrentUser_PropertyChanged;

                // Ustaw nową wartość i powiadom o zmianie
                SetProperty(ref _currentUser, value);

                // Subskrybuj zdarzenia od nowego obiektu, jeśli istnieje
                if (_currentUser != null) _currentUser.PropertyChanged += CurrentUser_PropertyChanged;

                // Powiadom o zmianie właściwości IsUserLoggedIn
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
        }

        /// <summary>
        /// Wskazuje, czy użytkownik jest aktualnie zalogowany.
        /// Zwraca true, jeśli CurrentUser nie jest nullem i posiada ważny token.
        /// </summary>
        public bool IsUserLoggedIn => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Token);

        /// <summary>
        /// Ustawia sesję użytkownika na podstawie danych otrzymanych po pomyślnym zalogowaniu.
        /// Tworzy nowy obiekt CurrentUserModel i przypisuje go do CurrentUser.
        /// </summary>
        /// <param name="userResponseDto">Obiekt DTO zawierający dane zalogowanego użytkownika i token.</param>
        public void SetUserSession(LoginSuccessResponse userResponseDto)
        {
            CurrentUser = CurrentUserModel.FromLoginSuccessResponse(userResponseDto);
        }

        /// <summary>
        /// Czyści sesję użytkownika, wylogowując go.
        /// Ustawia CurrentUser na null.
        /// </summary>
        public void ClearUserSession()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Aktualizuje URL zdjęcia profilowego w modelu bieżącego użytkownika.
        /// Setter w CurrentUserModel.ProfilePictureUrl zadba o domyślne zdjęcie, jeśli podasz null/puste.
        /// </summary>
        /// <param name="newUrl">Nowy URL zdjęcia profilowego. Może być null lub pusty, aby ustawić domyślne zdjęcie.</param>
        public void UpdateCurrentUserProfilePictureUrl(string? newUrl)
        {
            if (CurrentUser != null)
            {
                CurrentUser.ProfilePictureUrl = newUrl;
            }
        }

        /// <summary>
        /// Obsługuje zmiany właściwości w obiekcie CurrentUser.
        /// Ta metoda może być użyta do reakcji na zmiany w modelu użytkownika, ale obecna logika domyślnej ikony
        /// jest już w CurrentUserModel.
        /// </summary>
        private void CurrentUser_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

        }
    }
}