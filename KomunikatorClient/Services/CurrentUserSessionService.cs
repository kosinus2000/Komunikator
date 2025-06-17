using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorShared.DTOs;
using System.ComponentModel;

namespace KomunikatorClient.Services
{
    public class CurrentUserSessionService : ObservableObject
    {
        private CurrentUserModel? _currentUser;


        public CurrentUserModel? CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != null) _currentUser.PropertyChanged -= CurrentUser_PropertyChanged;
                SetProperty(ref _currentUser, value);
                if (_currentUser != null) _currentUser.PropertyChanged += CurrentUser_PropertyChanged;

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
        }

        public bool IsUserLoggedIn => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Token);

        public void SetUserSession(LoginSuccessResponse userResponseDto)
        {
            CurrentUser = CurrentUserModel.FromLoginSuccessResponse(userResponseDto);
        }

        public void ClearUserSession()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Aktualizuje URL zdjęcia profilowego w modelu bieżącego użytkownika.
        /// Setter w CurrentUserModel.ProfilePictureUrl zadba o domyślne zdjęcie, jeśli podasz null/puste.
        /// </summary>
        public void UpdateCurrentUserProfilePictureUrl(string? newUrl)
        {
            if (CurrentUser != null)
            {
                CurrentUser.ProfilePictureUrl = newUrl;
            }
        }

        private void CurrentUser_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Ta metoda nie wymaga zmian, bo logika domyślnej ikony jest w CurrentUserModel.
        }
    }
}