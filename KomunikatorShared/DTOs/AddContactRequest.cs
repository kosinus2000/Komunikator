namespace KomunikatorShared.DTOs;

/// <summary>
/// Reprezentuje żądanie dodania kontaktu dla użytkownika.
/// </summary>
public class AddContactRequest
{
    /// <summary>
    /// Pobiera lub ustawia unikalny identyfikator użytkownika powiązanego z żądaniem.
    /// </summary>
    /// <remarks>
    /// Ten identyfikator jednoznacznie identyfikuje użytkownika, którego kontakt jest dodawany.
    /// Jest używany do wykonywania operacji, takich jak pobieranie danych użytkownika lub walidacja żądań w systemie.
    /// </remarks>
    public string UserId { get; set; }

    /// <summary>
    /// Pobiera lub ustawia nazwę użytkownika kontaktu, który ma zostać dodany.
    /// </summary>
    /// <remarks>
    /// Ta właściwość reprezentuje unikalną nazwę użytkownika osoby, która ma zostać dodana jako kontakt.
    /// Jest używana, gdy kontakt jest identyfikowany na podstawie nazwy użytkownika zamiast unikalnego identyfikatora.
    /// Ta wartość jest wymagana w scenariuszach, w których identyfikator użytkownika nie jest dostępny.
    /// </remarks>
    public string Username { get; set;}
}