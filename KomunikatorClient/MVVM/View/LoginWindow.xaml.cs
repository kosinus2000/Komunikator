using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KomunikatorClient.Services;
using KomunikatorShared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using static KomunikatorClient.Services.AuthService;

namespace KomunikatorClient.MVVM.View
{
    /// <summary>
    /// Enum definiujący dostępne widoki okna logowania (Login, Register, PasswordReset).
    /// </summary>
    public enum CurrentView
    {
        Login,
        Register,
        PasswordReset
    }

    /// <summary>
    /// Okno odpowiadające za logowanie, rejestrację i resetowanie hasła w aplikacji.
    /// Obsługuje logikę interakcji użytkownika z formularzami.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;
        private readonly CurrentUserSessionService _currentUserSessionService;

        private CurrentView _currentView = CurrentView.Login;

        /// <summary>
        /// Aktualnie wyświetlany widok (Login, Register, PasswordReset).
        /// </summary>
        public CurrentView View
        {
            get => _currentView;
            set
            {
                _currentView = value;
                SetView(_currentView); // Wywołanie metody aktualizującej UI po zmianie widoku
            }
        }

        /// <summary>
        /// Ustawia widoczny widok (panel) na podstawie przekazanej wartości.
        /// Zmienia rozmiar okna i widoczność poszczególnych paneli.
        /// </summary>
        /// <param name="view">Widok do ustawienia.</param>
        public void SetView(CurrentView view)
        {
            // Ustawienie widoczności paneli i wysokości okna w zależności od wybranego widoku
            LoginStackPanel.Visibility = (view == CurrentView.Login) ? Visibility.Visible : Visibility.Hidden;
            RegisterStackPanel.Visibility = (view == CurrentView.Register) ? Visibility.Visible : Visibility.Hidden;
            PasswordResetStackPanel.Visibility = (view == CurrentView.PasswordReset) ? Visibility.Visible : Visibility.Hidden;

            // Logika dla przycisku "Wróć"
            bool isBackBtnVisible = (view == CurrentView.Register || view == CurrentView.PasswordReset);
            backBtn.Visibility = isBackBtnVisible ? Visibility.Visible : Visibility.Hidden;
            backBtn.IsEnabled = isBackBtnVisible;

            // Ustawienie wysokości okna
            if (view == CurrentView.Login)
            {
                Height = 320;
            }
            else if (view == CurrentView.Register)
            {
                Height = 370;
            }
            else if (view == CurrentView.PasswordReset)
            {
                Height = 250;
            }
        }

        /// <summary>
        /// Konstruktor domyślny. Inicjalizuje komponenty i ustawia domyślny widok.
        /// Pobiera serwisy z kontenera zależności.
        /// </summary>
        public LoginWindow() : this(App.ServiceProvider.GetRequiredService<AuthService>(),
            App.ServiceProvider.GetRequiredService<CurrentUserSessionService>())
        {
            // Wywołanie konstruktora z zależnościami.
            // Inicjalizacja komponentów (InitializeComponent()) i ustawienie widoku
            // są już w tym konstruktorze (patrz niżej).
        }

        /// <summary>
        /// Konstruktor z zależnościami, używany do wstrzykiwania serwisów.
        /// </summary>
        /// <param name="authService">Serwis autoryzacji użytkownika.</param>
        /// <param name="currentUserSessionService">Serwis sesji użytkownika.</param>
        public LoginWindow(AuthService authService, CurrentUserSessionService currentUserSessionService)
        {
            InitializeComponent(); // Inicjalizacja komponentów UI
            _authService = authService;
            _currentUserSessionService = currentUserSessionService;

            SetView(CurrentView.Login); // Ustawienie początkowego widoku na logowanie
            btnMaximize.IsEnabled = false; // Maksymalizacja okna jest zazwyczaj wyłączona dla okien dialogowych
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku logowania.
        /// Wysyła dane do serwisu uwierzytelniającego i otwiera główne okno aplikacji w przypadku sukcesu.
        /// </summary>
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string userNameLogin = UserNameRoundedTxtBoxLogin.Text;
            string userPasswordLogin = UserPasswordRoundedTxtBoxLogin.Password;

            LoginRequestModel sendingDataLogin = new LoginRequestModel
            {
                Username = userNameLogin,
                Password = userPasswordLogin
            };

            Log.Information("LoginWindow: Przygotowano dane do wysłania dla użytkownika {UserName}", sendingDataLogin.Username);

            try
            {
                bool respondSuccesed = await _authService.LoginAsync(sendingDataLogin);

                if (respondSuccesed)
                {
                    Log.Information("LoginWindow: Logowanie zakończone sukcesem dla użytkownika {Username}!", sendingDataLogin.Username);
                    // Pobranie MainWindow z kontenera zależności i pokazanie go
                    MainWindow mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                    this.Close(); // Zamknięcie okna logowania po pomyślnym zalogowaniu
                }
                else
                {
                    Log.Warning("LoginWindow: Logowanie nieudane dla użytkownika {Username}.", sendingDataLogin.Username);
                    MessageBox.Show("Logowanie nie powiodło się. Sprawdź wprowadzone dane lub spróbuj ponownie później.",
                        "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoginWindow: Wystąpił błąd podczas próby logowania użytkownika {Username}.", sendingDataLogin.Username);
                MessageBox.Show("Wystąpił nieprzewidziany błąd aplikacji.", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku rejestracji.
        /// Wysyła dane do serwisu uwierzytelniającego i wyświetla wynik operacji.
        /// Sprawdza zgodność haseł przed wysłaniem.
        /// </summary>
        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string userNameRegistration = UserNameRoundedTxtBoxRegistration.Text;
            string userEmailRegistration = UserEmailRoundedTxtBoxRegistration.Text;
            string userPasswordRegistration = UserPasswordRoundedTxtBoxRejestration.Password;
            string userPasswordConfirmation = UserPasswordRoundedTxtBoxRejestrationRepeat.Password;

            if (userPasswordRegistration != userPasswordConfirmation)
            {
                MessageBox.Show("Hasła różnią się od siebie.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Information);
                return; // Zakończ, jeśli hasła nie pasują
            }

            RegisterRequestModel registrationData = new RegisterRequestModel
            {
                Username = userNameRegistration,
                Email = userEmailRegistration,
                Password = userPasswordRegistration
            };

            try
            {
                RegistrationResultModel registrationResult = await _authService.RegisterAsync(registrationData);
                if (registrationResult.Succeeded)
                {
                    Log.Information("LoginWindow: Rejestracja zakończona sukcesem dla użytkownika {Username}!", registrationData.Username);
                    MessageBox.Show("Rejestracja pomyślna! Możesz się teraz zalogować.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetView(CurrentView.Login); // Po udanej rejestracji, przejdź do widoku logowania
                }
                else
                {
                    Log.Warning("LoginWindow: Rejestracja nieudana dla użytkownika {Username}.", registrationData.Username);
                    if (registrationResult.Errors != null && registrationResult.Errors.Any())
                    {
                        string errorMessages = string.Join("\n- ", registrationResult.Errors.Select(err => err.Description));
                        MessageBox.Show($"Rejestracja nie powiodła się z powodu:\n- {errorMessages}", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Rejestracja nie powiodła się. Sprawdź dane i spróbuj ponownie.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoginWindow: Wystąpił błąd podczas próby rejestracji użytkownika {Username}.", registrationData.Username);
                MessageBox.Show("Wystąpił nieprzewidziany błąd aplikacji podczas rejestracji.", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Umożliwia przeciąganie okna za pomocą lewego przycisku myszy.
        /// </summary>
        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Przełącza widok na panel resetowania hasła.
        /// </summary>
        private void Label_ResetujHaslo_Click(object sender, MouseButtonEventArgs e)
        {
            View = CurrentView.PasswordReset; // Użycie właściwości View do zmiany widoku
        }

        /// <summary>
        /// Przełącza widok na panel rejestracji.
        /// </summary>
        private void Label_Zarejestruj_Click(object sender, MouseButtonEventArgs e)
        {
            View = CurrentView.Register; // Użycie właściwości View do zmiany widoku
        }

        /// <summary>
        /// Minimalizuje okno.
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maksymalizuje lub przywraca okno do normalnego rozmiaru.
        /// </summary>
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            // Możliwość maksymalizacji okna logowania jest często niepożądana, ale jeśli jest potrzebna:
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        /// Zamyka całą aplikację.
        /// </summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Poniższe metody to zazwyczaj obsługa zdarzeń, które mogą być usunięte, jeśli nie są używane w XAML
        private void UserNameRoundedTxtBox_Loaded(object sender, RoutedEventArgs e) { /* Metoda pusta, można usunąć jeśli niepotrzebna */ }

        /// <summary>
        /// Powraca do widoku logowania.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            View = CurrentView.Login; // Użycie właściwości View do zmiany widoku
        }

        private void btnPasswordReset_Click(object sender, RoutedEventArgs e) { /* Metoda pusta, można usunąć jeśli niepotrzebna */ }

        /// <summary>
        /// Placeholder dla przycisku wysyłania resetowania hasła.
        /// </summary>
        private void btnRessetPasswordSend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Resetowanie hasła... (tu dodaj swój kod implementacji resetowania hasła, np. wywołanie metody w AuthService)");
            // Przykładowo, można tu dodać:
            // string email = UserEmailRoundedTxtBoxPasswordReset.Text;
            // bool success = await _authService.ResetPasswordAsync(email);
            // if (success) { MessageBox.Show("Instrukcje resetowania hasła zostały wysłane na Twój adres e-mail."); }
            // else { MessageBox.Show("Wystąpił błąd podczas resetowania hasła. Spróbuj ponownie."); }
        }

        private void UserNameRoundedTxtBoxRegistration_Loaded(object sender, RoutedEventArgs e) { /* Metoda pusta, można usunąć jeśli niepotrzebna */ }

        private void UserNameRoundedTxtBoxRegistration_TextChanged(object sender, TextChangedEventArgs e) { /* Metoda pusta, można usunąć jeśli niepotrzebna */ }
    }
}