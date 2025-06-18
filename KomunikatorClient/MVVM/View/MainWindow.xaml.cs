using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.MVVM.View;
using KomunikatorClient.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
        /// Dostawca usług do rozwiązywania zależności.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Inicjalizuje nowe wystąpienie klasy MainWindow.
        /// </summary>
        /// <param name="mainViewModel">ViewModel przekazany przez DI do powiązania z widokiem.</param>
        /// <param name="serviceProvider">Dostawca usług do rozwiązywania zależności.</param>
        public MainWindow(MainViewModel mainViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _serviceProvider = serviceProvider;
            this.DataContext = _mainViewModel;
            _ = _mainViewModel.LoadContactsAsync();
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
        }

        /// <summary>
        /// Obsługuje przycisk rozwinięcia menu kontekstowego w formie graficznej.
        /// </summary>
        private void btnUserMoreOptions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img != null && img.ContextMenu != null)
            {
                img.ContextMenu.DataContext = this.DataContext;
                img.ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Obsługuje żądanie wylogowania z aplikacji.
        /// </summary>
        public void OnLogoutRequested(object? sender, EventArgs e)
        {
            this.Close();
            LoginWindow loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku opcji kontaktu.
        /// </summary>
        private void BtnContactOptions_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_mainViewModel.SendCommand.CanExecute(null))
            {
                _mainViewModel.SendCommand.Execute(null);
            }
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku wysyłania wiadomości.
        /// </summary>
        private void btnSendMessage(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Otwiera okno dodawania nowego kontaktu.
        /// </summary>
        private void OpenAddContactWindow(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            try
            {
                var addContactWin = _serviceProvider.GetService<AddContactWindow>();

                if (addContactWin != null)
                {
                    addContactWin.ContactAdded += AddContactWindow_ContactAdded;
                    addContactWin.ShowDialog();
                    addContactWin.ContactAdded -= AddContactWindow_ContactAdded;
                }
                else
                {
                    MessageBox.Show("Nie udało się utworzyć okna dodawania kontaktu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Błąd podczas otwierania okna AddContactWindow.");
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Obsługuje zdarzenie dodania kontaktu z okna AddContactWindow.
        /// Odświeża listę kontaktów w MainViewModel.
        /// </summary>
        private async void AddContactWindow_ContactAdded(object? sender, ContactAddedEventArgs e)
        {
            if (e.AddedContact != null)
            {
                var newContactModel = ContactModel.FromDto(e.AddedContact);
                _mainViewModel.Contacts.Add(newContactModel);
                Log.Information("MainWindow: Kontakt {Username} dodany bezpośrednio do ObservableCollection.", e.AddedContact.Username);
            }
            else
            {
                Log.Information("MainWindow: Brak danych o dodanym kontakcie, odświeżam całą listę kontaktów.");
                await _mainViewModel.LoadContactsAsync();
            }
        }
    }
}