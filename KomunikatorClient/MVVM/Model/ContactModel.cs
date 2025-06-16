// MVVM/Model/ContactModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel; // Dodaj ten using
using System.Linq;
using System.Runtime.CompilerServices; // Dodaj ten using dla atrybutu CallerMemberName (opcjonalnie, ale dobre praktyki)

namespace KomunikatorClient.MVVM.Model
{
    /// <summary>
    /// Reprezentuje kontakt (użytkownika) w aplikacji czatu, wraz z przypisanymi wiadomościami.
    /// </summary>
    public class ContactModel : INotifyPropertyChanged // !!! Ważne: Implementacja INotifyPropertyChanged
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
                    OnPropertyChanged(); // Powiadomienie UI o zmianie
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
                    OnPropertyChanged(); // Powiadomienie UI o zmianie
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
                // Ważne: ObservableCollection sama powiadamia o zmianach w KOLEKCJI (dodawanie/usuwanie elementów),
                // ale przypisanie CAŁKOWICIE NOWEJ KOLEKCJI wymaga powiadomienia UI o zmianie właściwości 'Messages'.
                if (_messages != value)
                {
                    _messages = value;
                    OnPropertyChanged(); // Powiadomienie UI o zmianie, jeśli zmieniasz całą kolekcję
                    // Ponadto, ponieważ LastMessage zależy od Messages, powinniśmy też powiadomić o jego zmianie
                    OnPropertyChanged(nameof(LastMessage));
                }
            }
        }

        /// <summary>
        /// Ostatnia wiadomość w konwersacji z tym kontaktem lub "Brak wiadomości", jeśli brak danych.
        /// Używamy ?., aby bezpiecznie sprawdzić, czy Messages jest nullem.
        /// </summary>
        public string LastMessage => Messages?.Any() == true ? Messages.Last().Message : "Brak wiadomości";


        // --- Implementacja INotifyPropertyChanged ---
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
        /// !!! Ważne: Inicjalizuje kolekcję Messages, aby uniknąć NullReferenceException.
        /// </summary>
        public ContactModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            // Możesz też tutaj ustawić domyślną ImageSource, jeśli nie jest to zrobione w definicji właściwości
            // ImageSource = "/Images/icons8-male-user-64.png";

            // Subskrybujemy zdarzenie CollectionChanged, aby aktualizować LastMessage
            // gdy wiadomości są dodawane lub usuwane.
            Messages.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(LastMessage));
        }
    }
}