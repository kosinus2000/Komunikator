using System;

namespace KomunikatorClient.MVVM.Model
{
    /// <summary>
    /// Reprezentuje pojedynczą wiadomość w konwersacji czatowej.
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// Data i godzina wysłania wiadomości.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Nazwa użytkownika, który wysłał wiadomość.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Ścieżka do awatara użytkownika.
        /// </summary>
        public string ImageSource { get; set; }

        /// <summary>
        /// Treść wiadomości tekstowej.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Kolor przypisany do użytkownika, np. do wizualnego wyróżnienia wiadomości w UI.
        /// </summary>
        public string UserColor { get; set; }

        /// <summary>
        /// Wskazuje, czy wiadomość została wysłana przez lokalnego użytkownika (true), czy przez innego (false).
        /// </summary>
        public bool IsNativeOrigin { get; set; }

        /// <summary>
        /// Wskazuje, czy jest to pierwsza wiadomość w danej grupie (np. przy wyświetlaniu daty lub nazwiska).
        /// Może być null, jeśli informacja nie została określona.
        /// </summary>
        public bool? FirstMessage { get; set; }
    }
}
