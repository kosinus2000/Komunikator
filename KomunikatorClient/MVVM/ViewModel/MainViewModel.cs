﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.Services;

namespace KomunikatorClient.MVVM.ViewModel;

/// <summary>
/// Główny ViewModel aplikacji, zarządzający logiką okna głównego.
/// </summary>
public class MainViewModel : ObservableObject
{
    /// <summary>
    /// Kolekcja wiadomości wyświetlanych w aktywnym czacie.
    /// </summary>
    public ObservableCollection<MessageModel> Messages { get; set; }

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

    private ContactModel _selectedContact;

    /// <summary>
    /// Aktualnie wybrany kontakt; jego wiadomości są wyświetlane.
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
    /// Komenda do wylogowania użytkownika.
    /// </summary>
    public RelayCommand LogoutCommand { get; set; }

    /// <summary>
    /// Zdarzenie wywoływane po żądaniu wylogowania.
    /// </summary>
    public event EventHandler LogoutRequested;

    /// <summary>
    /// Inicjalizuje nowy egzemplarz klasy <see cref="MainViewModel"/>.
    /// Ustawia domyślne dane, wiąże komendy i subskrybuje zmiany sesji użytkownika.
    /// </summary>
    /// <param name="currentUserSessionService">Serwis zarządzający bieżącą sesją użytkownika.</param>
    public MainViewModel(CurrentUserSessionService currentUserSessionService)
    {
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();
        CurrentUserSessionService = currentUserSessionService;

        LogoutCommand = new RelayCommand(o =>
        {
            currentUserSessionService.ClearUserSession();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        });

        CurrentUserSessionService.PropertyChanged += CurrentUserSessionService_PropertyChanged;

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

        // Tymczasowe dane testowe
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
}