using System.Text.Json.Serialization;

namespace KomunikatorShared.DTOs
{
    /// <summary>
    /// Reprezentuje pojedynczy błąd zwrócony przez system tożsamości (Identity).
    /// </summary>
    public class IdentityErrorResponse
    {
        /// <summary>
        /// Kod błędu, np. "DuplicateUserName", "InvalidEmail" itp.
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Opis błędu — zazwyczaj czytelny komunikat dla użytkownika.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
