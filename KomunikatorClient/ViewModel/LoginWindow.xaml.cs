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
        throw new NotImplementedException();
    }

    private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
    
    private void btnRegister_Click(object sender, RoutedEventArgs e)
    {
        // Tutaj umieść kod, który ma się wykonać po kliknięciu przycisku
        // Na przykład:
        MessageBox.Show("Przycisk rejestracji został kliknięty");
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
}