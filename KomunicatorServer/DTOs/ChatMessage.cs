namespace KomunikatorServer.DTOs
{
    /// <summary>
    /// Reprezentuje wiadomość czatu przesyłaną między użytkownikami.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Unikalny identyfikator wiadomości.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identyfikator nadawcy wiadomości.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Obiekt użytkownika będącego nadawcą.
        /// </summary>
        public AppUser Sender { get; set; }

        /// <summary>
        /// Identyfikator odbiorcy wiadomości.
        /// </summary>
        public string ReceiverId { get; set; }

        /// <summary>
        /// Obiekt użytkownika będącego odbiorcą.
        /// </summary>
        public AppUser Receiver { get; set; }

        /// <summary>
        /// Treść wiadomości.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Znacznik czasu utworzenia wiadomości.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Wskazuje, czy wiadomość została odczytana.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Tworzy nową wiadomość z unikalnym identyfikatorem oraz domyślną datą i statusem.
        /// </summary>
        public ChatMessage()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            IsRead = false;
        }
    }
}