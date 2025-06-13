using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Serilog;

namespace KomunikatorClient.Services;

/// <summary>
/// Serwis odpowiedzialny za obsługę operacji związanych z użytkownikiem w aplikacji.
/// </summary>
public class UserService
{
    private readonly HttpClient _httpClient;
    private const string ServerApiBaseUrl = "https://localhost:7233";

    /// <summary>
    /// Serwis odpowiedzialny za obsługę operacji związanych z użytkownikiem w aplikacji.
    /// </summary>
    public UserService(CurrentUserSessionService currentUserSessionService, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Log.Information("UserService: HttpClient utworzony. BaseAddress: {BaseAddress}",
            _httpClient.BaseAddress?.ToString() ?? "null");
    }

    /// <summary>
    /// Asynchroniczna metoda odpowiedzialna za pobieranie listy kontaktów użytkownika
    /// z serwera przy użyciu żądania HTTP GET.
    /// </summary>
    /// <returns>
    /// Lista obiektów typu ContactDto reprezentujących kontakty użytkownika.
    /// Jeśli wystąpi błąd lub brak autoryzacji, zwracana jest pusta lista.
    /// </returns>
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        string endpointUrl = $"{ServerApiBaseUrl}/api/Contacts/getContacts";

        Log.Information("UserService:  ! === ROZPOCZYNAM GetContactsAsync === ! ");
        Log.Information("UserService: URL: {Url}", endpointUrl);

        var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
        if (authHeader == null)
        {
            Log.Error("UserService: BRAK TOKENU AUTORYZACJI!");
            return new List<ContactDto>();
        }

        Log.Information("UserService: Authorization Header: {Scheme} {Parameter}",
            authHeader.Scheme,
            authHeader.Parameter?.Length > 20
                ? $"{authHeader.Parameter.Substring(0, 10)}...{authHeader.Parameter.Substring(authHeader.Parameter.Length - 10)}"
                : authHeader.Parameter);

        try
        {
            Log.Information("UserService: Wysyłam żądanie HTTP GET...");

            var response = await _httpClient.GetAsync(endpointUrl);

            Log.Information("UserService: Otrzymano odpowiedź:");
            Log.Information("  - Status Code: {StatusCode} ({StatusCodeNumber})", response.StatusCode,
                (int)response.StatusCode);
            Log.Information("  - Reason Phrase: {ReasonPhrase}", response.ReasonPhrase ?? "null");
            Log.Information("  - Headers Count: {HeadersCount}", response.Headers.Count());

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
                    List<ContactDto>? contacts = JsonSerializer.Deserialize<List<ContactDto>>(rawContent,
                        new JsonSerializerOptions
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
            Log.Information("UserService:  ! === ZAKOŃCZONO GetContactsAsync === !");
        }
    }

    public async Task<bool> SendMessageAsync(SendMessageRequest request)
    {
        string endpointUrl = $"{ServerApiBaseUrl}/api/messages/send";
        Log.Information("UserService: Wysyłam żądanie POST wiadomości na adres: {Url}", endpointUrl);
        var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
        if (authHeader == null)
        {
            Log.Error("UserService: BRAK TOKENU AUTORYZACJI PRZY WYSYŁANIU WIADOMOŚCI!");
            return false;
        }

        Log.Information("UserService: Authorization Header: {Scheme} {Parameter}", authHeader.Scheme,
            authHeader.Parameter?.Length > 20
                ? $"{authHeader.Parameter.Substring(0, 10)}...{authHeader.Parameter.Substring(authHeader.Parameter.Length - 10)}"
                : authHeader.Parameter);
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpointUrl, request);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("UserService: Wiadomość pomyślnie wysłana. Status: {StatusCode}", response.StatusCode);
                return true;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "UserService: Wysyłanie wiadomości nie powiodło się. Status: {StatusCode}, Treść błędu: {ErrorContent}",
                    response.StatusCode, string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);
                List<IdentityErrorResponse>? errors = null;
                if (!string.IsNullOrEmpty(errorContent))
                {
                    try
                    {
                        errors = JsonSerializer.Deserialize<List<IdentityErrorResponse>>(errorContent);
                    }
                    catch (JsonException jsonEx)
                    {
                        Log.Error(jsonEx,
                            "UserService: Błąd deserializacji błędu wiadomości. Raw content: {RawContent}",
                            errorContent);
                    }
                }

                if (errors != null && errors.Any())
                {
                    string errorMessages = string.Join("\n- ", errors.Select(e => e.Description));
                    Log.Warning("UserService: Szczegółowe błędy wysyłania wiadomości z serwera:\n- {Errors}",
                        errorMessages);
                }

                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex,
                "UserService: Błąd HTTP podczas wysyłania wiadomości. Serwer może być nieosiągalny. URL: {Url}",
                endpointUrl);
            return false;
        }
        catch (JsonException ex)
        {
            Log.Error(ex,
                "UserService: Błąd JSON podczas wysyłania wiadomości (serializacja żądania lub deserializacja odpowiedzi).");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UserService: Nieoczekiwany błąd podczas wysyłania wiadomości.");
            return false;
        }
    }
}