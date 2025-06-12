using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Documents;
using KomunikatorServer.DTOs;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace KomunikatorClient.Services;

/// <summary>
/// Provides user-related operations and communicates with the server to retrieve data.
/// </summary>
public class UserService
{
    private readonly HttpClient _httpClient;
    private const string ServerApiBaseUrl = "https://localhost:7233";
    private readonly CurrentUserSessionService _currentUserSessionService;

    public UserService(CurrentUserSessionService currentUserSessionService, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _currentUserSessionService = currentUserSessionService;
    }

    /// <summary>
    /// Retrieves a list of user contacts from the server.
    /// </summary>
    /// <returns>
    /// A list of ContactDto objects representing the user's contacts.
    /// Returns an empty list if the request fails or encounters an error.
    /// </returns>
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        string endpointUrl = $"{ServerApiBaseUrl}/api/contacts/getContacts";
        Log.Information("UserService: Wysyłam żądanie GET po listę kontaktów na adres: {Url}", endpointUrl);

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
            if (response.IsSuccessStatusCode)
            {
                List<ContactDto>? contacts = await response.Content.ReadFromJsonAsync<List<ContactDto>>();
                Log.Information("UserService: Pomyślnie pobrano {Count} kontaktów.", contacts?.Count ?? 0);
                return contacts ?? new List<ContactDto>();
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "UserService: Nie udało się pobrać kontaktów. Status: {StatusCode}, Treść błędu: {ErrorContent}",
                    response.StatusCode, errorContent);
                return new List<ContactDto>();
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex,
                "UserService: Błąd żądania HTTP podczas pobierania kontaktów. Serwer może być nieosiągalny. URL: {Url}",
                endpointUrl);
            return new List<ContactDto>();
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "UserService: Błąd deserializacji JSON podczas pobierania kontaktów.");
            return new List<ContactDto>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UserService: Nieoczekiwany błąd podczas pobierania kontaktów.");
            return new List<ContactDto>();
        }
    }
}

