using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;
using Newtonsoft.Json;
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
                    List<ContactDto>? contacts = System.Text.Json.JsonSerializer.Deserialize<List<ContactDto>>(rawContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Log.Information("UserService: Pomyślnie zdeserializowano {Count} kontaktów", contacts?.Count ?? 0);
                    return contacts ?? new List<ContactDto>();
                }
                catch (System.Text.Json.JsonException jsonEx)
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
    public async Task UploadProfilePicture(byte[] imageData, string fileName)
    {
        var token = _currentUserSessionService.CurrentUser?.Token;
        if (string.IsNullOrEmpty(token))
        {
            Log.Warning("Próba wysłania zdjęcia profilowego bez tokena.");
            throw new InvalidOperationException("Użytkownik nie jest zalogowany.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageData);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png"); // Dostosuj do faktycznego typu obrazu
            content.Add(fileContent, "file", fileName); // "file" musi pasować do nazwy parametru w API serwera

            HttpResponseMessage response = await _httpClient.PostAsync("User/UploadProfilePicture", content); // Dostosuj URL do swojego API
            response.EnsureSuccessStatusCode();
            Log.Information("Zdjęcie profilowe wgrane.");
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Błąd HTTP podczas wgrywania zdjęcia.");
            throw new Exception("Błąd serwera podczas wgrywania zdjęcia.", ex);
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<string> GetProfilePictureUrl(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return "/Images/default_profile.png"; // Zwróć domyślny, jeśli brak nazwy użytkownika
        }

        var token = _currentUserSessionService.CurrentUser?.Token;
        if (string.IsNullOrEmpty(token))
        {
            Log.Warning("Próba pobrania zdjęcia profilowego bez tokena.");
            return "/Images/default_profile.png";
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {

            HttpResponseMessage response = await _httpClient.GetAsync($"User/ProfilePictureUrl?username={username}");
            response.EnsureSuccessStatusCode();

            // Jeśli serwer zwraca bezpośrednio URL jako string:
            string? url = await response.Content.ReadAsStringAsync();
            return url ?? "KomunikatorClient/Images/icons8-male-user-32.png";

            // Jeśli serwer zwraca JSON, np. { "profilePictureUrl": "..." }:
            // var result = await response.Content.ReadFromJsonAsync<YourProfilePictureUrlResponseDto>();
            // return result?.ProfilePictureUrl ?? "/Images/default_profile.png";
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Błąd HTTP podczas pobierania URL zdjęcia profilowego.");
            return "/Images/default_profile.png";
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null; // Wyczyść nagłówek
        }
    }
    public async Task<List<MessageDto>> GetConversationWithContact(string contactId)
    {
        var response = await _httpClient.GetAsync($"{ServerApiBaseUrl}/api/messages/conversation/{contactId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<MessageDto>>(content) ?? new List<MessageDto>();
    }

    public async Task SendMessageAsync(string receiverId, string messageText)
    {
        var messageDto = new
        {
            ReceiverId = receiverId,
            Content = messageText
        };

        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(messageDto),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync($"{ServerApiBaseUrl}/api/messages/send", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Log.Error("Błąd serwera przy wysyłaniu wiadomości: {StatusCode} - {Error}", response.StatusCode, error);
            throw new HttpRequestException($"Błąd {response.StatusCode}: {error}");
        }
    }

}