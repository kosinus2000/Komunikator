using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using KomunikatorClient.Core;
using KomunikatorClient.MVVM.Model;
using KomunikatorClient.Services;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Serilog;

namespace KomunikatorClient.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly UserService _userService;

        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactModel> Contacts { get; set; }
        public CurrentUserSessionService CurrentUserSessionService { get; }

        public RelayCommand SendCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        private ContactModel _selectedContact;

        public ContactModel SelectedContact
        {
            get => _selectedContact;
            set
            {
                _selectedContact = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayedContactName));
                // TODO: Tutaj, po zmianie SelectedContact, możesz załadować historię wiadomości
            }
        }

        public string DisplayedContactName
        {
            get => CurrentUserSessionService.CurrentUser?.Username ?? "@UserName";
        }

        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler LogoutRequested;

        public MainViewModel(CurrentUserSessionService currentUserSessionService, UserService userService)
        {
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();
            CurrentUserSessionService = currentUserSessionService;
            _userService = userService;

            LogoutCommand = new RelayCommand(o =>
            {
                currentUserSessionService.ClearUserSession();
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            });

            CurrentUserSessionService.PropertyChanged += CurrentUserSessionService_PropertyChanged;


            SendCommand = new RelayCommand(ExecuteSendCommand);
        }


        private async Task ExecuteSendCommand(object o)
        {
            if (string.IsNullOrWhiteSpace(Message) || SelectedContact == null)
            {
                Serilog.Log.Warning("MainViewModel: Próba wysłania pustej wiadomości lub brak wybranego kontaktu.");
                return;
            }

            var sendMessageRequest = new SendMessageRequest
            {
                ReceiverId = SelectedContact.Id,
                Content = Message
            };

            bool messageSent = await _userService.SendMessageAsync(sendMessageRequest);

            if (messageSent)
            {
                Messages.Add(new MessageModel
                {
                    Time = DateTime.Now,
                    Username = CurrentUserSessionService.CurrentUser?.Username ?? "Ja",
                    Message = Message,
                    IsNativeOrigin = true,
                    ImageSource = CurrentUserSessionService.CurrentUser?.ProfilePictureUrl ?? "/Images/icons8-male-user-64.png"
                });
                Message = string.Empty;
                Serilog.Log.Information("MainViewModel: Wiadomość pomyślnie dodana do UI i wysłana.");
            }
            else
            {
                Serilog.Log.Error("MainViewModel: Nie udało się wysłać wiadomości do serwera.");
            }
        }

        private void CurrentUserSessionService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentUserSessionService.CurrentUser) ||
                e.PropertyName == nameof(CurrentUserSessionService.IsUserLoggedIn))
            {
                OnPropertyChanged(nameof(DisplayedContactName));
                // TODO: Załaduj kontakty, gdy użytkownik się zaloguje (jeśli nie ładujesz w MainWindow_Loaded)
                // if (CurrentUserSessionService.IsUserLoggedIn)
                // {
                //     _ = LoadContactsAsync();
                // }
            }
        }

        public async Task LoadContactsAsync()
        {
            Serilog.Log.Information("MainViewModel: Rozpoczynam ładowanie kontaktów.");
            try
            {
                List<ContactDto> contactDtos = await _userService.GetContactsAsync();
                Contacts.Clear();
                if (contactDtos != null && contactDtos.Any())
                {
                    foreach (var dto in contactDtos)
                    {
                        ContactModel model = MapContactDtoToModel(dto);
                        Contacts.Add(model);
                    }
                    Serilog.Log.Information("MainViewModel: Pomyślnie załadowano {Count} kontaktów.", Contacts.Count);
                }
                else
                {
                    Serilog.Log.Information("MainViewModel: Nie znaleziono żadnych kontaktów lub lista jest pusta.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "MainViewModel: Błąd podczas ładowania kontaktów.");
            }
        }

        private ContactModel MapContactDtoToModel(ContactDto contactDto)
        {
            ContactModel contactModel = new ContactModel
            {
                Id = contactDto.Id,
                Username = contactDto.Username,
                DisplayName = contactDto.DisplayName,
                ImageSource = contactDto.ProfilePictureUrl ?? "/Images/icons8-male-user-64.png",
                Messages = new ObservableCollection<MessageModel>()
            };
            return contactModel;
        }
    }
}