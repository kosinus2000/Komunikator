namespace KomunikatorShared.DTOs
{
    /// <summary>
    /// Reprezentuje model danych używany do żądania logowania użytkownika.
    /// Zawiera nazwę użytkownika i hasło potrzebne do uwierzytelnienia.
    /// </summary>
    public class LoginRequestModel
    {
        /// <summary>
        /// Pobiera lub ustawia nazwę użytkownika, która ma zostać użyta do logowania.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Pobiera lub ustawia hasło, które ma zostać użyte do logowania.
        /// </summary>
        public string Password { get; set; }
    }
}