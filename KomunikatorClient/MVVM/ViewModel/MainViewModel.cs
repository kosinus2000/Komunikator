using System.Collections.ObjectModel;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.Services;
using System.ComponentModel; // Dodaj ten using!

namespace KomunikatorClient.MVVM.ViewModel;

public class MainViewModel : ObservableObject
{
    public ObservableCollection<MessageModel> Messages { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }
    public CurrentUserSessionService CurrentUserSessionService { get; }
    public RelayCommand SendCommand { get; set; }

    private ContactModel _selectedContact;

    public ContactModel SelectedContact
    {
        get { return _selectedContact; }
        set
        {
            _selectedContact = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedContactName));
        }
    }

    public string DisplayedContactName
    {
        get { return CurrentUserSessionService.CurrentUser?.Username ?? "@UserName"; }
    }


    private string _message;

    public string Message
    {
        get { return _message; }
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel(CurrentUserSessionService currentUserSessionService)
    {
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();
        CurrentUserSessionService = currentUserSessionService;

       
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


    private void CurrentUserSessionService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentUserSessionService.CurrentUser) ||
            e.PropertyName == nameof(CurrentUserSessionService.IsUserLoggedIn))
        {
            OnPropertyChanged(nameof(DisplayedContactName)); 
        }
    }
    
}