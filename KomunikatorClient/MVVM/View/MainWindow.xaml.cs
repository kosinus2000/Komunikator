using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KomunikatorClient.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace KomunikatorClient.MVVM.View
{
    /// <summary>
    /// Główne okno aplikacji klienta komunikatora.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Główny ViewModel powiązany z tym oknem.
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// Inicjalizuje nowe wystąpienie klasy MainWindow.
        /// </summary>
        /// <param name="mainViewModel">ViewModel przekazany przez DI do powiązania z widokiem.</param>
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            this.DataContext = _mainViewModel;
            _ = _mainViewModel.LoadContactsAsync(); //Wywołanie asynchrnonicznej metody dodoawania kontaktów w konstruktorze,
                                                    //który  nie może być asynch
            _mainViewModel.LogoutRequested += OnLogoutRequested;
        }

        /// <summary>
        /// Umożliwia przeciąganie okna po ekranie po wciśnięciu lewego przycisku myszy.
        /// </summary>
        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Minimalizuje okno aplikacji.
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Przełącza okno między stanem normalnym a zmaksymalizowanym.
        /// </summary>
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

        /// <summary>
        /// Zamyka aplikację.
        /// </summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Obsługuje zmianę zaznaczenia w liście (np. zmiana aktywnego czatu).
        /// Aktualnie pusta – możesz tu dodać logikę np. przełączenia widoku.
        /// </summary>
        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // TODO: Dodaj tutaj logikę zmiany zawartości głównego panelu na podstawie wyboru z listy.
        }

        /// <summary>
        /// Obsługuje przycisk rozwinięcia menu kontekstowego w formie graficznej 
        /// 
        /// </summary>
        private void btnUserMoreOptions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img != null && img.ContextMenu != null)
            {
                // Ustaw DataContext dla ContextMenu
                img.ContextMenu.DataContext = this.DataContext;
                img.ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Obsługuje zdarzenie żądania wylogowania, zamykając główne okno i otwierając okno logowania.
        /// </summary>
        /// <param name="sender">Źródło zdarzenia, które wywołało żądanie wylogowania.</param>
        /// <param name="e">Dane zdarzenia związane z żądaniem wylogowania.</param>
        public void OnLogoutRequested(object? sender, EventArgs e)
        {
            this.Close();
            LoginWindow loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}