namespace KomunikatorClient.DTOs
{
    /// <summary>
    /// Reprezentuje odpowiedź z informacją o błędzie zwracaną z serwera.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Opis błędu zwrócony przez serwer.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Kod błędu przypisany przez serwer (np. 400, 401, 500).
        /// </summary>
        public int ErrorCode { get; set; }
    }
}
