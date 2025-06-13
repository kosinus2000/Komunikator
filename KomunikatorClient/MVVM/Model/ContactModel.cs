using System.Collections.ObjectModel;
using System.Linq;

namespace KomunikatorClient.MVVM.Model
{
    /// <summary>
    /// Reprezentuje kontakt (użytkownika) w aplikacji czatu, wraz z przypisanymi wiadomościami.
    /// </summary>
    public class ContactModel
    {
        /// <summary>
        /// Unikalny identyfikator kontaktu.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nazwa użytkownika (login lub wyświetlana nazwa kontaktu).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Wyświetlana nazwa kontaktu, alternatywna wobec nazwy użytkownika.
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Ścieżka do awatara kontaktu (domyślnie obrazek użytkownika).
        /// </summary>
        public string ImageSource { get; set; } = "/Images/icons8-male-user-64.png";

        /// <summary>
        /// Kolekcja wiadomości powiązanych z tym kontaktem.
        /// </summary>
        public ObservableCollection<MessageModel> Messages { get; set; }

        /// <summary>
        /// Ostatnia wiadomość w konwersacji z tym kontaktem lub "Brak wiadomości", jeśli brak danych.
        /// </summary>
        public string LastMessage => Messages?.Any() == true ? Messages.Last().Message : "Brak wiadomości";
    }
}
