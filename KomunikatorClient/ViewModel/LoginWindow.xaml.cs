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

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        LoginRequestModel sendingDates = new LoginRequestModel();
        string userName = UserNameRoundedTxtBox.Text;
        string userPassword = UserPasswordRoundedTxtBox.Password;
        
        sendingDates.Username = userName;
        sendingDates.Password = userPassword;
        MessageBox.Show($"Dane gotowe do wysłania:\nUżytkownik: {sendingDates.Username}\nHasło: [dla bezpieczeństwa nie pokazujemy]", "Dane logowania");

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