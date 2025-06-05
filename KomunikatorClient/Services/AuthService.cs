using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using System.Net.Http.Headers;
using KomunikatorClient.DTOs;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.Services
{
    /// <summary>
    /// Klasa serwisowa odpowiedzialna za operacje związane z uwierzytelnianiem użytkowników,
    /// takie jak logowanie i rejestracja.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string ServerApiBaseUrl = "https://localhost:7233";
        private readonly CurrentUserSessionService _currentUserSessionService;

        /// <summary>
        /// Konstruktor klasy AuthService.
        /// Inicjalizuje instancję HttpClient oraz wstrzykuje usługę zarządzającą sesją użytkownika.
        /// </summary>
        /// <param name="currentUserSessionService">Serwis zarządzający bieżącą sesją użytkownika.</param>
        public AuthService(CurrentUserSessionService currentUserSessionService)
        {
            // Inicjalizacja HttpClient z domyślnym handlerem.
            _httpClient = new HttpClient(new HttpClientHandler());
            _currentUserSessionService = currentUserSessionService;
        }

        /// <summary>
        /// Ustawia nagłówek autoryzacji "Bearer" z podanym tokenem dla wszystkich przyszłych żądań HttpClient.
        /// Jeśli token jest pusty lub null, nagłówek autoryzacji jest usuwany.
        /// </summary>
        /// <param name="token">Token autoryzacyjny (JWT) do ustawienia.</param>
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
        /// Usuwa nagłówek autoryzacji z HttpClient, efektywnie wylogowując użytkownika lub resetując stan autoryzacji.
        /// </summary>
        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Log.Debug("AuthService: Usunięto nagłówek autoryzacji.");
        }

        /// <summary>
        /// Wysyła żądanie logowania do serwera.
        /// Próbuje zalogować użytkownika za pomocą dostarczonych danych logowania.
        /// </summary>
        /// <param name="loginRequestModel">Model danych żądania logowania zawierający nazwę użytkownika i hasło.</param>
        /// <returns>
        /// <c>true</c> jeśli logowanie powiodło się i otrzymano poprawny token; w przeciwnym razie <c>false</c>.
        /// </returns>
        public async Task<bool> LoginAsync(LoginRequestModel loginRequestModel)
        {
            string path = "/api/auth/login";
            string loginEndpointUrl = $"{ServerApiBaseUrl}{path}";
            Log.Debug("AuthService: Pełny URL do logowania: {Url}", loginEndpointUrl);

            try
            {
                // Automatyczna serializacja obiektu loginRequestModel do JSON i wysłanie jako treść żądania POST.
                Log.Information(
                    "AuthService: Wysyłanie żądania POST (używając PostAsJsonAsync) na adres: {Url} dla użytkownika {Username}",
                    loginEndpointUrl, loginRequestModel.Username);

                // Wysyłanie danych na wskazany endpoint. Await sprawia, że aplikacja czeka na zakończenie operacji,
                // jednocześnie nie blokując głównego wątku.
                HttpResponseMessage httpResponse =
                    await _httpClient.PostAsJsonAsync(loginEndpointUrl, loginRequestModel);

                if (httpResponse.IsSuccessStatusCode)
                {
                    Log.Information(
                        "AuthService: Logowanie przebiegło pomyślnie dla użytkownika {Username}. Status: {StatusCode}",
                        loginRequestModel.Username, httpResponse.StatusCode);

                    LoginSuccessResponse? loginResponse = null;
                    try
                    {
                        // Deserializacja odpowiedzi JSON do obiektu LoginSuccessResponse.
                        loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
                    }
                    catch (JsonException jsonEx)
                    {
                        Log.Error(jsonEx, "AuthService: Błąd deserializacji LoginSuccessResponse. Treść odpowiedzi: {Content}",
                                      await httpResponse.Content.ReadAsStringAsync());

                        Log.Warning("AuthService: Nie udało się zdeserializować LoginSuccessResponse. Sprawdź, czy serwer zwraca poprawny format JSON.");
                        return false;
                    }

                    // Sprawdzenie, czy odpowiedź zawiera token i dane użytkownika.
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        // Zapisanie danych sesji użytkownika.
                        _currentUserSessionService.SetUserSession(loginResponse);
                        Log.Information("AuthService: Użytkownik {Username} zalogowany pomyślnie. Token: {Token}",
                                        loginResponse.Username, loginResponse.Token);
                        // Ustawienie nagłówka autoryzacji z otrzymanym tokenem.
                        SetAuthorizationHeader(loginResponse.Token);
                        Log.Information("AuthService: Dane sesji użytkownika dla {Username} zapisane.", loginResponse.Username);
                        return true;
                    }
                    else
                    {
                        Log.Warning("AuthService: Logowanie pomyślne (status {StatusCode}), ale token lub dane użytkownika są puste.",
                                        httpResponse.StatusCode);
                        return false;
                    }
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();

                    Log.Warning(
                        "AuthService: Logowanie nie powiodło się dla użytkownika: {Username}, Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        loginRequestModel.Username, httpResponse.StatusCode,
                        string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

                    List<IdentityErrorResponse>? errors = null;
                    if (!string.IsNullOrEmpty(errorContent))
                        try
                        {
                            // Próba deserializacji treści błędu jako lista błędów tożsamości.
                            errors = JsonSerializer.Deserialize<List<IdentityErrorResponse>>(errorContent);
                        }
                        catch (JsonException jj)
                        {
                            Log.Error(jj,
                                "AuthService: Nie udało się zdeserializować treści błędu jako List<IdentityErrorResponse>. Treść {ErrorContent}",
                                errorContent);
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
                Log.Error(e,
                    "AuthService: Błąd żądania HTTP podczas logowania dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
                    loginRequestModel.Username, loginEndpointUrl);
                return false;
            }
            catch (JsonException e)
            {
                Log.Error(e, "AuthService: Błąd JSON podczas logowania dla {Username}. Problem z serializacją żądania lub deserializacją odpowiedzi.",
                    loginRequestModel.Username);
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, "AuthService: Nieoczekiwany błąd podczas logowania użytkownika {Username}",
                    loginRequestModel.Username);
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
                string jsonPayload = JsonSerializer.Serialize(registerRequestModel);
                Log.Information(
                    "AuthService: Wysyłanie żądania POST do rejestracji na adres: {Url} dla użytkownika {Username}",
                    registerEndpointUrl, registerRequestModel.Username);

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
                    Log.Warning(
                        "AuthService: Nieudana rejestracja dla użytkownika: {Username}, Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        registerRequestModel.Username, httpResponse.StatusCode,
                        string.IsNullOrEmpty(await httpResponse.Content.ReadAsStringAsync())
                            ? "[Brak]"
                            : await httpResponse.Content.ReadAsStringAsync());

                    List<IdentityErrorResponse>? errors = null;
                    try
                    {
                        // Próba deserializacji treści błędu jako lista błędów tożsamości.
                        errors = await httpResponse.Content.ReadFromJsonAsync<List<IdentityErrorResponse>>();
                    }
                    catch (JsonException jj)
                    {
                        Log.Error(jj,
                            "AuthService: Nie udało się zdeserializować treści błędu jako List<IdentityErrorResponse>.",
                            await httpResponse.Content.ReadAsStringAsync());
                    }

                    result.Succeeded = false;
                    result.Errors = errors;
                    return result;
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error(e,
                    "AuthService: Błąd żądania http podczas rejestracji dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
                    registerRequestModel.Username, registerEndpointUrl);
                return new RegistrationResultModel { Succeeded = false };
            }
            catch (JsonException e)
            {
                Log.Error("AuthService: Błąd wysłanego json podczas przygotowania żądania rejestracji dla {Username}",
                    registerRequestModel.Username);
                return new RegistrationResultModel { Succeeded = false };
            }
            catch (Exception e)
            {
                Log.Error("AuthService: Nieoczekiwany bład podczas rejestracji użytkownika {Username}",
                    registerRequestModel.Username);
                return new RegistrationResultModel { Succeeded = false };
            }
        }
    }
}