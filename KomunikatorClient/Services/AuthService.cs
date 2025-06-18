using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using KomunikatorClient.DTOs;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.Services
{
    /// <summary>
    /// Serwis odpowiedzialny za operacje autoryzacji i uwierzytelniania, takie jak logowanie i rejestracja.
    /// Komunikuje się z API serwera.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        // Bazowy URL API serwera.
        private const string ServerApiBaseUrl = "https://localhost:7233";
        private readonly CurrentUserSessionService _currentUserSessionService;

        /// <summary>
        /// Inicjalizuje nową instancję klasy AuthService.
        /// </summary>
        /// <param name="currentUserSessionService">Serwis do zarządzania bieżącą sesją użytkownika.</param>
        /// <param name="httpClient">Instancja HttpClient do wykonywania żądań HTTP.</param>
        public AuthService(CurrentUserSessionService currentUserSessionService, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _currentUserSessionService = currentUserSessionService;
        }

        /// <summary>
        /// Ustawia nagłówek autoryzacji "Bearer" dla wszystkich przyszłych żądań HTTP.
        /// </summary>
        /// <param name="token">Token autoryzacyjny. Jeśli null lub pusty, nagłówek zostanie usunięty.</param>
        public void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Log.Debug("AuthService: Ustawiono nagłówek autoryzacji z tokenem.");
            }
            else
            {
                ClearAuthorizationHeader();
            }
        }

        /// <summary>
        /// Usuwa nagłówek autoryzacji z HttpClient.
        /// </summary>
        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Log.Debug("AuthService: Usunięto nagłówek autoryzacji.");
        }

        /// <summary>
        /// Wysyła żądanie logowania do serwera.
        /// </summary>
        /// <param name="loginRequestModel">Model danych logowania zawierający nazwę użytkownika i hasło.</param>
        /// <returns>True, jeśli logowanie powiodło się i sesja użytkownika została ustawiona; w przeciwnym razie false.</returns>
        public async Task<bool> LoginAsync(LoginRequestModel loginRequestModel)
        {
            string path = "/api/auth/login";
            string loginEndpointUrl = $"{ServerApiBaseUrl}{path}";
            Log.Debug("AuthService: Pełny URL do logowania: {Url}", loginEndpointUrl);

            try
            {
                Log.Information("AuthService: Wysyłanie żądania POST (JSON) na adres: {Url} dla użytkownika {Username}",
                    loginEndpointUrl, loginRequestModel.Username);

                HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(loginEndpointUrl, loginRequestModel);

                if (httpResponse.IsSuccessStatusCode)
                {
                    Log.Information("AuthService: Logowanie przebiegło pomyślnie dla użytkownika {Username}. Status: {StatusCode}",
                        loginRequestModel.Username, httpResponse.StatusCode);

                    LoginSuccessResponse? loginResponse = null;
                    try
                    {
                        loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
                    }
                    catch (JsonException jsonEx)
                    {
                        Log.Error(jsonEx, "AuthService: Błąd deserializacji LoginSuccessResponse. Treść odpowiedzi: {Content}",
                            await httpResponse.Content.ReadAsStringAsync());
                        return false;
                    }

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        _currentUserSessionService.SetUserSession(loginResponse);
                        SetAuthorizationHeader(loginResponse.Token);
                        Log.Information("AuthService: Użytkownik {Username} zalogowany pomyślnie. Token: {Token}",
                            loginResponse.Username, loginResponse.Token);
                        return true;
                    }
                    else
                    {
                        Log.Warning("AuthService: Logowanie pomyślne, ale token lub dane użytkownika są puste w odpowiedzi.");
                        return false;
                    }
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    Log.Warning("AuthService: Logowanie nie powiodło się dla użytkownika {Username}. Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        loginRequestModel.Username, httpResponse.StatusCode, string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

                    // Próba deserializacji błędów, jeśli są dostępne
                    List<IdentityErrorResponse>? errors = null;
                    if (!string.IsNullOrEmpty(errorContent))
                        try
                        {
                            errors = JsonSerializer.Deserialize<List<IdentityErrorResponse>>(errorContent);
                        }
                        catch (JsonException jj)
                        {
                            Log.Error(jj, "AuthService: Nie udało się zdeserializować treści błędu jako List<IdentityErrorResponse>.");
                        }

                    if (errors != null && errors.Any())
                    {
                        string errorMessages = string.Join("\n- ", errors.Select(e => e.Description));
                        Log.Warning("AuthService: Szczegółowe błędy logowania z serwera:\n- {Errors}", errorMessages);
                    }
                    return false;
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, "AuthService: Błąd żądania HTTP podczas logowania dla {Username}. Serwer może być nieosiągalny. Sprawdź adres: {Url}",
                    loginRequestModel.Username, loginEndpointUrl);
                return false;
            }
            catch (JsonException e)
            {
                Log.Error(e, "AuthService: Błąd deserializacji JSON podczas logowania dla {Username}.", loginRequestModel.Username);
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, "AuthService: Nieoczekiwany błąd podczas logowania użytkownika {Username}", loginRequestModel.Username);
                return false;
            }
        }

        /// <summary>
        /// Reprezentuje wynik operacji rejestracji, wskazując, czy zakończyła się sukcesem
        /// oraz dostarczając listę błędów w przypadku niepowodzenia.
        /// </summary>
        public class RegistrationResultModel
        {
            /// <summary>
            /// Wskazuje, czy rejestracja powiodła się.
            /// </summary>
            public bool Succeeded { get; set; }
            /// <summary>
            /// Lista błędów tożsamości zwróconych przez serwer w przypadku niepowodzenia rejestracji.
            /// </summary>
            public List<IdentityErrorResponse>? Errors { get; set; }
        }

        /// <summary>
        /// Wysyła żądanie rejestracji nowego użytkownika do serwera.
        /// </summary>
        /// <param name="registerRequestModel">Model danych żądania rejestracji zawierający dane nowego użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="RegistrationResultModel"/> wskazujący, czy rejestracja powiodła się,
        /// oraz ewentualne błędy.
        /// </returns>
        public async Task<RegistrationResultModel> RegisterAsync(RegisterRequestModel registerRequestModel)
        {
            string path = "/api/auth/register";
            string registerEndpointUrl = $"{ServerApiBaseUrl}{path}";
            Log.Debug("AuthService: Pełny URL do rejestracji: {Url}", registerEndpointUrl);

            try
            {
                // Serializacja payloadu do logów (nie jest wymagane do działania, ale pomocne przy debugowaniu)
                string jsonPayload = JsonSerializer.Serialize(registerRequestModel);
                Log.Information(
                    "AuthService: Wysyłanie żądania POST do rejestracji na adres: {Url} dla użytkownika {Username}. Payload: {Payload}",
                    registerEndpointUrl, registerRequestModel.Username, jsonPayload);

                HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(registerEndpointUrl,
                    registerRequestModel);

                var result = new RegistrationResultModel();

                if (httpResponse.IsSuccessStatusCode)
                {
                    Log.Information(
                        "AuthService: Rejestracja przebiegła pomyślnie dla użytkownika {Username}. Status: {StatusCode}",
                        registerRequestModel.Username, httpResponse.StatusCode);
                    result.Succeeded = true;
                    return result;
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    Log.Warning(
                        "AuthService: Nieudana rejestracja dla użytkownika: {Username}, Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        registerRequestModel.Username, httpResponse.StatusCode,
                        string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

                    List<IdentityErrorResponse>? errors = null;
                    try
                    {
                        // Próba deserializacji treści błędu jako lista błędów tożsamości.
                        errors = await httpResponse.Content.ReadFromJsonAsync<List<IdentityErrorResponse>>();
                    }
                    catch (JsonException jj)
                    {
                        Log.Error(jj,
                            "AuthService: Nie udało się zdeserializować treści błędu jako List<IdentityErrorResponse>. Treść: {Content}",
                            errorContent); // Logowanie faktycznej treści, która spowodowała błąd deserializacji
                    }

                    result.Succeeded = false;
                    result.Errors = errors;
                    return result;
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error(e,
                    "AuthService: Błąd żądania HTTP podczas rejestracji dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
                    registerRequestModel.Username, registerEndpointUrl);
                return new RegistrationResultModel { Succeeded = false };
            }
            catch (JsonException e)
            {
                // Ten błąd może wystąpić, jeśli problem jest z serializacją registerRequestModel
                Log.Error(e, "AuthService: Błąd serializacji JSON podczas przygotowania żądania rejestracji dla {Username}",
                    registerRequestModel.Username);
                return new RegistrationResultModel { Succeeded = false };
            }
            catch (Exception e)
            {
                Log.Error(e, "AuthService: Nieoczekiwany błąd podczas rejestracji użytkownika {Username}",
                    registerRequestModel.Username);
                return new RegistrationResultModel { Succeeded = false };
            }
        }
    }
}