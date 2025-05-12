using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KomunikatorClient.Models;
using KomunikatorClient.Services;
using Serilog;

namespace Komunikator;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        btnMaximize.Visibility = Visibility.Hidden;
    }

    private async  void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        string userName = UserNameRoundedTxtBox.Text;
        string userPassword = UserPasswordRoundedTxtBox.Password;

        LoginRequestModel sendingData = new LoginRequestModel
        {
            Username = userName,
            Password = userPassword
        };
        Log.Information("LoginWindow: Przygotowano dane do wysłania dla użytkownika {UserName}", sendingData.Username);
        
        AuthService authService = new AuthService();
        try
        {
            bool respondSuccesed = await authService.LoginAsync(sendingData);

            if (respondSuccesed)
            {
                Log.Information("LoginWindow: Logowanie zakończone sukcesem dla użytkownika {Username}!",
                    sendingData.Username);
                MessageBox.Show("Logowanie pomyślne!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Log.Warning("LoginWindow: Logowanie nieudane dla użytkownika {Username}.", sendingData.Username);
                MessageBox.Show("Logowanie nie powiodło się. Sprawdź wprowadzone dane lub spróbuj ponownie później.",
                    "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "LoginWindow: Wystąpił błąd podczas próby logwoania użytkownika {Username}.",sendingData.Username);
            MessageBox.Show("Wystąpił nieprzewidziany błąd aplikacji. ", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void Label_ResetujHaslo_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Resetowanie hasła... (tu dodaj swój kod)");
    }

    private void Label_Zarejestruj_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Rejestracja... (tu dodaj swój kod)");
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
}