using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.Services;
using KomunikatorServer.DTOs;
using Microsoft.Win32;
using Serilog;

namespace KomunikatorClient.MVVM.ViewModel;

/// <summary>
/// Główny ViewModel aplikacji, zarządzający logiką okna głównego.
/// Obsługuje kontakty, wiadomości, wysyłanie wiadomości, zmianę zdjęcia profilowego i wylogowanie.
/// </summary>
public class MainViewModel : ObservableObject
{
    private readonly UserService _userService;

    /// <summary>
    /// Kolekcja wiadomości wyświetlanych w aktywnym czacie.
    /// Jest pusta, ponieważ wiadomości są przechowywane w SelectedContact.Messages.
    /// </summary>
    public ObservableCollection<MessageModel> Messages { get; set; } // Może być usunięta, jeśli wiadomości są ładowane do SelectedContact.Messages

    /// <summary>
    /// Kolekcja kontaktów użytkownika.
    /// </summary>
    public ObservableCollection<ContactModel> Contacts { get; set; }

    /// <summary>
    /// Serwis przechowujący informacje o aktualnie zalogowanym użytkowniku.
    /// </summary>
    public CurrentUserSessionService CurrentUserSessionService { get; }

    /// <summary>
    /// Komenda do wysyłania wiadomości.
    /// </summary>
    public RelayCommand SendCommand { get; set; }

    /// <summary>
    /// Komenda do zmiany zdjęcia profilowego.
    /// </summary>
    public RelayCommand ChangeProfilePictureCommand { get; set; }

    private ContactModel _selectedContact;

    /// <summary>
    /// Aktualnie wybrany kontakt; jego wiadomości są wyświetlane.
    /// Ustawienie tej właściwości powoduje załadowanie wiadomości dla wybranego kontaktu.
    /// </summary>
    public ContactModel SelectedContact
    {
        get => _selectedContact;
        set
        {
            if (_selectedContact != value)
            {
                _selectedContact = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayedContactName));
                // Natychmiastowe ładowanie wiadomości po zmianie wybranego kontaktu
                _ = OnSelectedContactChanged();
            }
        }
    }

    /// <summary>
    /// Metoda wywoływana po zmianie wybranego kontaktu.
    /// Odpowiada za asynchroniczne ładowanie wiadomości dla nowego kontaktu.
    /// </summary>
    private async Task OnSelectedContactChanged()
    {
        await LoadMessagesForSelectedContactAsync();
    }

    /// <summary>
    /// Nazwa kontaktu do wyświetlenia na górze czatu.
    /// </summary>
    public string DisplayedContactName =>
       _selectedContact?.Username ?? " ";

    private string _message;

    /// <summary>
    /// Treść wiadomości wpisywanej przez użytkownika w polu tekstowym.
    /// </summary>
    public string CurrentMessage
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Komenda do wylogowania użytkownika.
    /// </summary>
    public RelayCommand LogoutCommand { get; set; }

    /// <summary>
    /// Zdarzenie wywoływane po żądaniu wylogowania.
    /// </summary>
    public event EventHandler? LogoutRequested;

    /// <summary>
    /// Inicjalizuje nowy egzemplarz klasy <see cref="MainViewModel"/>.
    /// Ustawia domyślne dane, wiąże komendy i subskrybuje zmiany sesji użytkownika.
    /// </summary>
    /// <param name="currentUserSessionService">Serwis zarządzający bieżącą sesją użytkownika.</param>
    /// <param name="userService">Serwis do interakcji z API użytkowników i kontaktów.</param>
    public MainViewModel(CurrentUserSessionService currentUserSessionService, UserService userService)
    {
        Messages = new ObservableCollection<MessageModel>(); // Możliwe do usunięcia
        Contacts = new ObservableCollection<ContactModel>();
        CurrentUserSessionService = currentUserSessionService;
        _userService = userService;

        if (CurrentUserSessionService.IsUserLoggedIn)
        {
            // Ładowanie kontaktów asynchronicznie przy starcie ViewModelu
            _ = LoadContactsAsync();
        }

        // Definicja komendy wylogowania
        LogoutCommand = new RelayCommand(o =>
        {
            currentUserSessionService.ClearUserSession();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        });

        // Subskrypcja zmian w serwisie sesji użytkownika
        CurrentUserSessionService.PropertyChanged += CurrentUserSessionService_PropertyChanged;

        // Definicja komendy wysyłania wiadomości
        SendCommand = new RelayCommand(async o =>
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage) || SelectedContact == null)
            {
                Log.Warning("Próba wysłania pustej wiadomości lub brak wybranego kontaktu.");
                return;
            }

            // Oznaczanie pierwszej wiadomości w konwersacji (do stylizowania np. dymków)
            bool isFirst = !SelectedContact.Messages.Any();

            var newMessage = new MessageModel
            {
                Message = CurrentMessage,
                Time = DateTime.Now,
                Username = CurrentUserSessionService.CurrentUser?.Username ?? "Unknown",
                ImageSource = CurrentUserSessionService.CurrentUser?.ProfilePictureUrl ?? "/Images/icons8-male-user-32.png",
                UserColor = "#597D35", // Przykładowy kolor
                IsNativeOrigin = true,
                FirstMessage = isFirst
            };

            // Dodanie wiadomości do lokalnej kolekcji przed wysłaniem
            SelectedContact.Messages.Add(newMessage);

            try
            {
                // Wysłanie wiadomości przez UserService
                await _userService.SendMessageAsync(SelectedContact.Id, CurrentMessage);
                Log.Information("Wiadomość wysłana do {ContactId}: {Message}", SelectedContact.Id, CurrentMessage);
            }
            catch (Exception ex)
            {
                // Usunięcie wiadomości z lokalnej kolekcji w przypadku błędu
                SelectedContact.Messages.Remove(newMessage);
                Serilog.Log.Error(ex, "Błąd przy wysyłaniu wiadomości do {ContactId}", SelectedContact.Id);
            }

            // Wyczyszczenie pola wiadomości
            CurrentMessage = string.Empty;
        });

        // Definicja komendy zmiany zdjęcia profilowego
        ChangeProfilePictureCommand = new RelayCommand(async o => await ChangeProfilePicture());

        // Początkowe ustawienie nazwy wyświetlanego kontaktu
        OnPropertyChanged(nameof(DisplayedContactName));
    }

    /// <summary>
    /// Otwiera okno dialogowe wyboru pliku, wysyła wybrane zdjęcie do serwera
    /// i aktualizuje URL zdjęcia profilowego bieżącego użytkownika.
    /// </summary>
    private async Task ChangeProfilePicture()
    {
        if (CurrentUserSessionService.CurrentUser == null || string.IsNullOrEmpty(CurrentUserSessionService.CurrentUser.Username))
        {
            Serilog.Log.Warning("Próba zmiany zdjęcia profilowego bez zalogowanego użytkownika.");
            return;
        }

        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Obrazy (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Wszystkie pliki (*.*)|*.*",
            Title = "Wybierz nowe zdjęcie profilowe"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                string filePath = openFileDialog.FileName;
                byte[] imageData = File.ReadAllBytes(filePath);

                // Przesłanie zdjęcia do serwera
                await _userService.UploadProfilePicture(imageData, Path.GetFileName(filePath));

                // Pobranie zaktualizowanego URL zdjęcia profilowego
                string newProfilePictureUrl = await _userService.GetProfilePictureUrl(CurrentUserSessionService.CurrentUser.Username);

                // Aktualizacja URL-a w serwisie sesji użytkownika
                CurrentUserSessionService.UpdateCurrentUserProfilePictureUrl(newProfilePictureUrl);

                // Odświeżenie listy kontaktów, aby upewnić się, że inne miejsca w UI też odzwierciedlają zmianę
                await LoadContactsAsync();
                Serilog.Log.Information("Zaktualizowano zdjęcie profilowe użytkownika na: {ProfilePictureUrl}", newProfilePictureUrl);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Błąd podczas zmiany zdjęcia profilowego.");
                // Można wyświetlić komunikat dla użytkownika
                // MessageBox.Show($"Wystąpił błąd podczas zmiany zdjęcia profilowego: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Reaguje na zmiany właściwości w serwisie sesji użytkownika, np. po zalogowaniu lub wylogowaniu.
    /// </summary>
    /// <param name="sender">Obiekt, który wywołał zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia, zawierające nazwę zmienionej właściwości.</param>
    private void CurrentUserSessionService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentUserSessionService.CurrentUser) ||
            e.PropertyName == nameof(CurrentUserSessionService.IsUserLoggedIn))
        {
            OnPropertyChanged(nameof(DisplayedContactName));
        }
    }

    /// <summary>
    /// Ładuje listę kontaktów użytkownika z serwera i aktualizuje kolekcję Contacts.
    /// </summary>
    public async Task LoadContactsAsync()
    {
        Log.Information("MainViewModel: Rozpoczynam ładowanie kontaktów.");
        try
        {
            List<ContactDto> contactDtos = await _userService.GetContactsAsync();
            Contacts.Clear(); // Wyczyść bieżącą listę przed dodaniem nowych

            if (contactDtos != null && contactDtos.Any())
            {
                foreach (var dto in contactDtos)
                {
                    ContactModel model = MapContactDtoToModel(dto);
                    Contacts.Add(model);
                }
                Log.Information("MainViewModel: Pomyślnie załadowano {Count} kontaktów.", Contacts.Count);
            }
            else
            {
                Log.Information("MainViewModel: Nie znaleziono żadnych kontaktów lub lista jest pusta.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "MainViewModel: Błąd podczas ładowania kontaktów.");
            // Można dodać obsługę błędu np. wyświetlenie komunikatu dla użytkownika
        }
    }

    /// <summary>
    /// Mapuje obiekt ContactDto na ContactModel.
    /// </summary>
    /// <param name="contactDto">Obiekt DTO kontaktu.</param>
    /// <returns>Obiekt modelu kontaktu.</returns>
    private ContactModel MapContactDtoToModel(ContactDto contactDto)
    {
        return new ContactModel
        {
            Id = contactDto.Id,
            Username = contactDto.Username,
            ImageSource = string.IsNullOrEmpty(contactDto.ProfilePictureUrl) ? "/Images/icons8-male-user-32.png" : contactDto.ProfilePictureUrl,
            Messages = new ObservableCollection<MessageModel>() // Inicjalizacja pustej kolekcji wiadomości dla nowego kontaktu
        };
    }

    /// <summary>
    /// Ładuje wiadomości dla aktualnie wybranego kontaktu.
    /// </summary>
    private async Task LoadMessagesForSelectedContactAsync()
    {
        if (_selectedContact == null || CurrentUserSessionService.CurrentUser == null)
        {
            Log.Information("Nie wybrano kontaktu lub użytkownik nie jest zalogowany. Nie można załadować wiadomości.");
            return;
        }

        try
        {
            // Pobranie wiadomości z serwera
            var messages = await _userService.GetConversationWithContact(_selectedContact.Id);
            _selectedContact.Messages.Clear(); // Wyczyść bieżące wiadomości kontaktu

            for (int i = 0; i < messages.Count; i++)
            {
                var msg = messages[i];
                // Dodanie wiadomości do kolekcji SelectedContact.Messages
                _selectedContact.Messages.Add(new MessageModel
                {
                    Message = msg.Content,
                    Time = msg.Timestamp.ToLocalTime(), // Konwersja czasu na lokalny
                    Username = msg.IsMine
                        ? CurrentUserSessionService.CurrentUser.Username
                        : _selectedContact.Username,
                    ImageSource = msg.IsMine
                        ? CurrentUserSessionService.CurrentUser.ProfilePictureUrl
                        : _selectedContact.ImageSource,
                    FirstMessage = (i == 0), // Oznaczanie pierwszej wiadomości w konwersacji
                    UserColor = "#597D35", // Przykładowy kolor
                    IsNativeOrigin = true // Wszystkie wiadomości ładowane z serwera uznajemy za "natywne"
                });
            }

            Log.Information("Załadowano {Count} wiadomości z kontaktem {Contact}.", _selectedContact.Messages.Count, _selectedContact.Username);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Błąd podczas ładowania wiadomości z kontaktem {Contact}.", _selectedContact.Username);
            // Można dodać obsługę błędu np. wyświetlenie komunikatu dla użytkownika
        }
    }
}