using System.Collections.ObjectModel;
using System.Net.Mime;
using KomunikatorClient.MVVM.Model;

namespace KomunikatorClient.MVVM.View;

public partial class MainViewModel
{
    public ObservableCollection<MessageModel> Messages { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }

    public MainViewModel()
    {
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();

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