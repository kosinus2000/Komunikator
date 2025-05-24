using System.Collections.ObjectModel;
using System.Net.Mime;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;

namespace KomunikatorClient.MVVM.View;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<MessageModel> Messages { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }

    public RelayCommand SendCommand { get; set; }

    private ContactModel _selectedContact;

    public ContactModel SelectedContact
    {
        get { return _selectedContact; }
        set
        {
            _selectedContact = value;
            OnPropertyChanged();
        }
    }

    private string _message;

    public string Message
    {
        get { return _message; }
        set
        {
            {
                _message = value;
            }
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();

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
    }
}