using System.Windows;
using System.Windows.Input;
using KomunikatorClient.MVVM.ViewModel;

namespace KomunikatorClient.MVVM.View
{
    
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            this.DataContext = _mainViewModel;
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                
                if (this.WindowState == WindowState.Maximized)
                {
                    // Opcjonalnie: Jeśli okno jest zmaksymalizowane, przy kliknięciu paska tytułowego
                    // i przeciągnięciu, możesz przywrócić je do normalnego rozmiaru i zacząć przeciągać.
                    // Wymaga to bardziej złożonej logiki, aby obliczyć nową pozycję.
                    // Na razie, można po prostu wyjść z metody lub przywrócić do normalnego stanu.
                    // this.WindowState = WindowState.Normal;
                }

                this.DragMove();
            }
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

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
