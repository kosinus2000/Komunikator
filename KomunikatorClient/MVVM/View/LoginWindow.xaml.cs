using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KomunikatorClient.Services;
using KomunikatorShared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using static KomunikatorClient.Services.AuthService;

namespace KomunikatorClient.MVVM.View;

public enum View
{
    Login,
    Register,
    PasswordReset
}

public partial class LoginWindow : Window
{

    private AuthService _authService;
    private CurrentUserSessionService _currentUserSessionService;


    // --- Zarządzanie oknami ---
    private View _view = View.Login;

    public View View
    {
        get => _view;
        set => _view = value;
    }

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
    public LoginWindow() : this(App.ServiceProvider.GetRequiredService<AuthService>(), 
        App.ServiceProvider.GetRequiredService<CurrentUserSessionService>()) 
    {
        this.InitializeComponent(); // Explicitly call the correct method
        SetView(View.Login);
        btnMaximize.IsEnabled = false;
    }

    public LoginWindow(AuthService authService, CurrentUserSessionService currentUserSessionService)
    {
        this.InitializeComponent(); // Explicitly call the correct method
        SetView(View.Login);
        btnMaximize.IsEnabled = false;
        _authService = authService;
        _currentUserSessionService = currentUserSessionService;
    }
    //--------------------------------------------------------------------------------------------------------------------

        // --- Metody dla logowaina ---
    private async void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        // --- Logowanie ---
        string userNameLogin = UserNameRoundedTxtBoxLogin.Text;
        string userPasswordLogin = UserPasswordRoundedTxtBoxLogin.Password;


        // --- Resetowanie hasła
        string userEmailPasswordReset = UserEmailRoundedTxtBoxPasswordReset.Text;

        LoginRequestModel sendingDataLogin = new LoginRequestModel
        {
            Username = userNameLogin,
            Password = userPasswordLogin
        };

        Log.Information("LoginWindow: Przygotowano dane do wysłania dla użytkownika {UserName}",
            sendingDataLogin.Username);

     
        try
        {
            bool respondSuccesed = await _authService.LoginAsync(sendingDataLogin);

            if (respondSuccesed)
            {
                Log.Information("LoginWindow: Logowanie zakończone sukcesem dla użytkownika {Username}!",
                    sendingDataLogin.Username);
              
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
            Log.Error(ex, "LoginWindow: Wystąpił błąd podczas próby logwoania użytkownika {Username}.",
                sendingDataLogin.Username);
            MessageBox.Show("Wystąpił nieprzewidziany błąd aplikacji. ", "Błąd krytyczny", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------

    // --- Metody dla rejestracji ---
    private async void btnRegister_Click(object sender, RoutedEventArgs e)
    {
        Log.Information("btnRegister_Click: Próba pobrania danych z formularza rejestracji.");
        string userNameRegistration = UserNameRoundedTxtBoxRegistration.Text;
        string userEmailRegistration = UserEmailRoundedTxtBoxRegistration.Text; 
        string userPasswordRegistration = UserPasswordRoundedTxtBoxRejestration.Password;
        string userPasswordConfirmation = UserPasswordRoundedTxtBoxRejestrationRepeat.Password;
        Log.Information($"btnRegister_Click: UserName: '{userNameRegistration}', Email: '{userEmailRegistration}', Password: '{userPasswordRegistration}', ConfirmPassword: '{userPasswordConfirmation}'");

        RegisterRequestModel registrationData = new RegisterRequestModel
        {
            Username = userNameRegistration,
            Email = userEmailRegistration,
            Password = userPasswordRegistration
        };
        Log.Information("LoginWindow: Przygotowano dane do rejestracji dla użytkownika {UserName}",
            registrationData.Username);

        AuthService authService = _authService;
        if (userPasswordRegistration == userPasswordConfirmation)
        {
            try
            {
                RegistrationResultModel registrationResult = await authService.RegisterAsync(registrationData);
                if (registrationResult.Succeeded)
                {
                    Log.Information("LoginWindow: Rejestracja zakończone sukcesem dla użytkownika {Username}!",
                        registrationData.Username);
                    MessageBox.Show("Rejestracja pomyślna! Możesz się teraz zalogować.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetView(View.Login);
                }
                else
                {
                    Log.Warning("LoginWindow: Rejestracja nieudana dla użytkownika {Username}.",
                        registrationData.Username);
                    if (registrationResult.Errors != null && registrationResult.Errors.Any())
                    {
                        string errorMessages = string.Join("\n- ", registrationResult.Errors.Select(err => err.Description));
                        MessageBox.Show($"Rejestracja nie powiodła się z powodu:\n{errorMessages}", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Rejestracja nie powiodła się. Sprawdź wprowadzone dane lub spróbuj ponownie później.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoginWindow: Wystąpił błąd podczas próby rejestracji użytkownika {Username}.",
                    registrationData.Username);
                MessageBox.Show("Wystąpił nieprzewidziany błąd aplikacji podczas rejestracji.", "Błąd krytyczny", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show("Hasła różnią się od siebie", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }


    private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void Label_ResetujHaslo_Click(object sender, MouseButtonEventArgs e)
    {
        SetView(View.PasswordReset);
    }

    private void Label_Zarejestruj_Click(object sender, MouseButtonEventArgs e)
    {
        SetView(View.Register);
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        switch (this.WindowState)
        {
            case WindowState.Normal:
                this.WindowState = WindowState.Maximized;
                break;
            case WindowState.Maximized:
                this.WindowState = WindowState.Normal;
                break;
        }
    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void UserNameRoundedTxtBox_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        SetView(View.Login);
    }

    private void btnPasswordReset_Click(object sender, RoutedEventArgs e)
    {
    }

    private void btnRessetPasswordSend_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Resetowanie hasła... (tu dodaj swój kod)");
    }

    private void UserNameRoundedTxtBoxRegistration_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void UserNameRoundedTxtBoxRegistration_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}