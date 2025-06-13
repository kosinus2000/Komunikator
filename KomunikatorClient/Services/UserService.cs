using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KomunikatorServer.DTOs;
using Serilog;

namespace KomunikatorClient.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private const string ServerApiBaseUrl = "https://localhost:7233";
    private readonly CurrentUserSessionService _currentUserSessionService;

    public UserService(CurrentUserSessionService currentUserSessionService, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _currentUserSessionService = currentUserSessionService;

        Log.Information("UserService: HttpClient utworzony. BaseAddress: {BaseAddress}",
            _httpClient.BaseAddress?.ToString() ?? "null");
    }

    public void SetAuthToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Log.Information("UserService: Token ustawiony. Długość: {Length} znaków", token.Length);

            // Debug: pokaż pierwsze i ostatnie znaki tokenu
            var tokenPreview = token.Length > 20 ?
                $"{token.Substring(0, 10)}...{token.Substring(token.Length - 10)}" :
                token;
            Log.Information("UserService: Token preview: {TokenPreview}", tokenPreview);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Log.Warning("UserService: Token usunięty (pusty)");
        }
    }

    public async Task<List<ContactDto>> GetContactsAsync()
    {
        // POPRAWIONY URL - wielka litera C w Contact!
        string endpointUrl = $"{ServerApiBaseUrl}/api/Contacts/getContacts";

        Log.Information("UserService: === ROZPOCZYNAM GetContactsAsync ===");
        Log.Information("UserService: URL: {Url}", endpointUrl);

        // Sprawdź stan autoryzacji
        var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
        if (authHeader == null)
        {
            Log.Error("UserService: BRAK TOKENU AUTORYZACJI!");
            return new List<ContactDto>();
        }

        Log.Information("UserService: Authorization Header: {Scheme} {Parameter}",
            authHeader.Scheme,
            authHeader.Parameter?.Length > 20 ?
                $"{authHeader.Parameter.Substring(0, 10)}...{authHeader.Parameter.Substring(authHeader.Parameter.Length - 10)}" :
                authHeader.Parameter);

        try
        {
            Log.Information("UserService: Wysyłam żądanie HTTP GET...");

            var response = await _httpClient.GetAsync(endpointUrl);

            Log.Information("UserService: Otrzymano odpowiedź:");
            Log.Information("  - Status Code: {StatusCode} ({StatusCodeNumber})", response.StatusCode, (int)response.StatusCode);
            Log.Information("  - Reason Phrase: {ReasonPhrase}", response.ReasonPhrase ?? "null");
            Log.Information("  - Headers Count: {HeadersCount}", response.Headers.Count());

            // Wyświetl wszystkie headery odpowiedzi dla debugowania
            foreach (var header in response.Headers)
            {
                Log.Information("  - Response Header: {Key} = {Value}", header.Key, string.Join(", ", header.Value));
            }

            if (response.IsSuccessStatusCode)
            {
                string rawContent = await response.Content.ReadAsStringAsync();
                Log.Information("UserService: Raw response content: {Content}", rawContent);

                try
                {
                    List<ContactDto>? contacts = JsonSerializer.Deserialize<List<ContactDto>>(rawContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Log.Information("UserService: Pomyślnie zdeserializowano {Count} kontaktów", contacts?.Count ?? 0);
                    return contacts ?? new List<ContactDto>();
                }
                catch (JsonException jsonEx)
                {
                    Log.Error(jsonEx, "UserService: Błąd deserializacji JSON. Raw content: {RawContent}", rawContent);
                    return new List<ContactDto>();
                }
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("UserService: Żądanie nieudane!");
                Log.Error("  - Status: {StatusCode}", response.StatusCode);
                Log.Error("  - Error Content: {ErrorContent}", errorContent);

                // Szczególne traktowanie błędu 401 Unauthorized
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Log.Error("UserService: BŁĄD AUTORYZACJI - sprawdź token JWT!");
                }

                return new List<ContactDto>();
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "UserService: HttpRequestException - problem z połączeniem");
            return new List<ContactDto>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UserService: Nieoczekiwany błąd");
            return new List<ContactDto>();
        }
        finally
        {
            Log.Information("UserService: === ZAKOŃCZONO GetContactsAsync ===");
        }
    }
}