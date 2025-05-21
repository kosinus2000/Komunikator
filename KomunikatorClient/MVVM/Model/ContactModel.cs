using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KomunikatorClient.MVVM.Model
{
    public class ContactModel
    {
        public string Username { get; set; }

        public string ImageSource { get; set; } = "/Images/icons8-male-user-64.png";

        public ObservableCollection<MessageModel> Messages { get; set; }
        public string LastMessage => Messages.Any() ? Messages.Last().Message : "Brak wiadomości";
    }
}
