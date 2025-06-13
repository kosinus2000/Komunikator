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
    /// Enum definiujący dostępne widoki okna logowania.
    /// </summary>
    public enum View
    {
        Login,
        Register,
        PasswordReset
    }

    /// <summary>
    /// Okno odpowiadające za logowanie, rejestrację i resetowanie hasła.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private AuthService _authService;
        private CurrentUserSessionService _currentUserSessionService;

        private View _view = View.Login;

        /// <summary>
        /// Aktualnie wyświetlany widok (Login, Register, PasswordReset).
        /// </summary>
        public View View
        {
            get => _view;
            set => _view = value;
        }

        /// <summary>
        /// Ustawia widoczny widok (panel) na podstawie przekazanej wartości.
        /// </summary>
        /// <param name="view">Widok do ustawienia.</param>
        public void SetView(View view)
        {
            if (view == View.Login)
            {
                Height = 320;
                LoginStackPanel.Visibility = Visibility.Visible;
                RegisterStackPanel.Visibility = Visibility.Hidden;
                PasswordResetStackPanel.Visibility = Visibility.Hidden;
                backBtn.Visibility = Visibility.Hidden;
                backBtn.IsEnabled = false;
            }
            else if (view == View.Register)
            {
                Height = 370;
                LoginStackPanel.Visibility = Visibility.Hidden;
                RegisterStackPanel.Visibility = Visibility.Visible;
                PasswordResetStackPanel.Visibility = Visibility.Hidden;
                backBtn.Visibility = Visibility.Visible;
                backBtn.IsEnabled = true;
            }
            else if (view == View.PasswordReset)
            {
                Height = 250;
                LoginStackPanel.Visibility = Visibility.Hidden;
                RegisterStackPanel.Visibility = Visibility.Hidden;
                PasswordResetStackPanel.Visibility = Visibility.Visible;
                backBtn.Visibility = Visibility.Visible;
                backBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Konstruktor domyślny. Inicjalizuje komponenty i ustawia domyślny widok.
        /// </summary>
        public LoginWindow() : this(App.ServiceProvider.GetRequiredService<AuthService>(),
            App.ServiceProvider.GetRequiredService<CurrentUserSessionService>())
        {
            this.InitializeComponent();
            SetView(View.Login);
            btnMaximize.IsEnabled = false;
        }

        /// <summary>
        /// Konstruktor z zależnościami.
        /// </summary>
        /// <param name="authService">Serwis autoryzacji użytkownika.</param>
        /// <param name="currentUserSessionService">Serwis sesji użytkownika.</param>
        public LoginWindow(AuthService authService, CurrentUserSessionService currentUserSessionService)
        {
            this.InitializeComponent();
            SetView(View.Login);
            btnMaximize.IsEnabled = false;
            _authService = authService;
            _currentUserSessionService = currentUserSessionService;
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku logowania.
        /// Wysyła dane do serwisu i otwiera główne okno przy sukcesie.
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
                    MainWindow mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                    this.Close();
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
        /// Wysyła dane do serwisu i wyświetla wynik operacji.
        /// </summary>
        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string userNameRegistration = UserNameRoundedTxtBoxRegistration.Text;
            string userEmailRegistration = UserEmailRoundedTxtBoxRegistration.Text;
            string userPasswordRegistration = UserPasswordRoundedTxtBoxRejestration.Password;
            string userPasswordConfirmation = UserPasswordRoundedTxtBoxRejestrationRepeat.Password;

            RegisterRequestModel registrationData = new RegisterRequestModel
            {
                Username = userNameRegistration,
                Email = userEmailRegistration,
                Password = userPasswordRegistration
            };

            if (userPasswordRegistration == userPasswordConfirmation)
            {
                try
                {
                    RegistrationResultModel registrationResult = await _authService.RegisterAsync(registrationData);
                    if (registrationResult.Succeeded)
                    {
                        Log.Information("LoginWindow: Rejestracja zakończona sukcesem dla użytkownika {Username}!", registrationData.Username);
                        MessageBox.Show("Rejestracja pomyślna! Możesz się teraz zalogować.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        SetView(View.Login);
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
            else
            {
                MessageBox.Show("Hasła różnią się od siebie.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Umożliwia przeciąganie okna.
        /// </summary>
        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Przełącza widok na resetowanie hasła.
        /// </summary>
        private void Label_ResetujHaslo_Click(object sender, MouseButtonEventArgs e)
        {
            SetView(View.PasswordReset);
        }

        /// <summary>
        /// Przełącza widok na rejestrację.
        /// </summary>
        private void Label_Zarejestruj_Click(object sender, MouseButtonEventArgs e)
        {
            SetView(View.Register);
        }

        /// <summary>
        /// Minimalizuje okno.
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maksymalizuje lub przywraca okno.
        /// </summary>
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        /// Zamyka okno.
        /// </summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UserNameRoundedTxtBox_Loaded(object sender, RoutedEventArgs e) { }

        /// <summary>
        /// Powraca do widoku logowania.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SetView(View.Login);
        }

        private void btnPasswordReset_Click(object sender, RoutedEventArgs e) { }

        /// <summary>
        /// Placeholder przycisku resetowania hasła.
        /// </summary>
        private void btnRessetPasswordSend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Resetowanie hasła... (tu dodaj swój kod)");
        }

        private void UserNameRoundedTxtBoxRegistration_Loaded(object sender, RoutedEventArgs e) { }

        private void UserNameRoundedTxtBoxRegistration_TextChanged(object sender, TextChangedEventArgs e) { }
    }
}
