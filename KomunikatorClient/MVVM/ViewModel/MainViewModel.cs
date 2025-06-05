using System.Collections.ObjectModel;
using System.ComponentModel;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.Services;

namespace KomunikatorClient.MVVM.ViewModel;

/// <summary>
/// Główny ViewModel aplikacji – odpowiada za logikę okna głównego.
/// </summary>
public class MainViewModel : ObservableObject
{
    /// <summary>
    /// Lista wiadomości wyświetlana w aktualnym czacie.
    /// </summary>
    public ObservableCollection<MessageModel> Messages { get; set; }

    /// <summary>
    /// Lista kontaktów użytkownika.
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

    private ContactModel _selectedContact;

    /// <summary>
    /// Aktualnie wybrany kontakt – jego wiadomości są wyświetlane.
    /// </summary>
    public ContactModel SelectedContact
    {
        get => _selectedContact;
        set
        {
            _selectedContact = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedContactName));
        }
    }

    /// <summary>
    /// Nazwa kontaktu do wyświetlenia na górze czatu.
    /// </summary>
    public string DisplayedContactName =>
        CurrentUserSessionService.CurrentUser?.Username ?? "@UserName";

    private string _message;

    /// <summary>
    /// Treść wiadomości wpisywanej przez użytkownika.
    /// </summary>
    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Konstruktor inicjalizujący dane, wiążący komendy oraz subskrybujący zmiany sesji użytkownika.
    /// </summary>
    public MainViewModel(CurrentUserSessionService currentUserSessionService)
    {
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();
        CurrentUserSessionService = currentUserSessionService;

        // Obsługa zmian sesji użytkownika
        CurrentUserSessionService.PropertyChanged += CurrentUserSessionService_PropertyChanged;

        // Definicja komendy wysyłania wiadomości
        SendCommand = new RelayCommand(o =>
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            Messages.Add(new MessageModel
            {
                Message = Message,
                FirstMessage = false,
            });

            Message = string.Empty;
        });

        // Wiadomość powitalna (tymczasowe dane testowe)
        Messages.Add(new MessageModel
        {
            Time = DateTime.Now,
            Username = "User1",
            ImageSource = "user1.png",
            Message = "Hello!",
            UserColor = "#FF0000",
            IsNativeOrigin = false,
            FirstMessage = true
        });

        // Przykładowi użytkownicy
        for (int i = 0; i < 5; i++)
        {
            Contacts.Add(new ContactModel
            {
                Username = $"Andrzej {i}",
                Messages = Messages
            });
        }

        OnPropertyChanged(nameof(DisplayedContactName));
    }

    /// <summary>
    /// Reaguje na zmiany użytkownika w sesji (np. po zalogowaniu).
    /// </summary>
    private void CurrentUserSessionService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentUserSessionService.CurrentUser) ||
            e.PropertyName == nameof(CurrentUserSessionService.IsUserLoggedIn))
        {
            OnPropertyChanged(nameof(DisplayedContactName));
        }
    }
}
