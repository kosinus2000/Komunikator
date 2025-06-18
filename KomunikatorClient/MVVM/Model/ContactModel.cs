using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.MVVM.Model
{
    /// <summary>
    /// Reprezentuje kontakt (użytkownika) w aplikacji czatu, wraz z przypisanymi wiadomościami.
    /// </summary>
    public class ContactModel : INotifyPropertyChanged
    {
        private string _username;
        /// <summary>
        /// Nazwa użytkownika (login lub wyświetlana nazwa kontaktu).
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _imageSource;
        /// <summary>
        /// Ścieżka do awatara kontaktu (domyślnie obrazek użytkownika).
        /// </summary>
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<MessageModel> _messages;
        /// <summary>
        /// Kolekcja wiadomości powiązanych z tym kontaktem.
        /// </summary>
        public ObservableCollection<MessageModel> Messages
        {
            get => _messages;
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastMessage));
                }
            }
        }

        /// <summary>
        /// Ostatnia wiadomość w konwersacji z tym kontaktem lub "Brak wiadomości", jeśli brak danych.
        /// </summary>
        public string LastMessage => Messages?.Any() == true ? Messages.Last().Message : "Brak wiadomości";

        /// <summary>
        /// Globalny identyfikator kontaktu (użytkownika).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Adres e-mail użytkownika.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Data rejestracji użytkownika.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Wyświetlana nazwa kontaktu (opcjonalna).
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Metoda fabryczna do tworzenia ContactModel z ContactDto.
        /// </summary>
        /// <param name="dto">Obiekt ContactDto do konwersji.</param>
        /// <returns>Nowa instancja ContactModel.</returns>
        public static ContactModel FromDto(ContactDto dto)
        {
            return new ContactModel
            {
                Id = dto.Id,
                Username = dto.Username ?? "Nieznany",
                Email = dto.Email,
                RegistrationDate = dto.RegistrationDate,
                DisplayName = dto.DisplayName ?? dto.Username,
                ImageSource = dto.ProfilePictureUrl ?? "/Images/icons8-male-user-32.png",
                Messages = new ObservableCollection<MessageModel>()
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Metoda pomocnicza do wywoływania zdarzenia PropertyChanged.
        /// Używa CallerMemberName, aby automatycznie pobrać nazwę właściwości.
        /// </summary>
        /// <param name="propertyName">Nazwa właściwości, która się zmieniła.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Konstruktor ContactModel.
        /// </summary>
        public ContactModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Messages.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(LastMessage));
        }
    }
}