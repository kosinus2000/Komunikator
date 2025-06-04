using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using System.Net.Http.Json;
using KomunikatorClient.DTOs;
using KomunikatorShared.DTOs;
using System.Net.Http.Headers;

namespace KomunikatorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string ServerApiBaseUrl = "https://localhost:7233";
        private readonly CurrentUserSessionService _currentUserSessionService;

        // Konstruktor AuthService, który inicjalizuje HttpClient
        public AuthService(CurrentUserSessionService currentUserSessionService)
        {
            var handler = new HttpClientHandler();

            _httpClient = new HttpClient(handler);

            _currentUserSessionService = currentUserSessionService;
        }
        public void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Ustawia domyślny nagłówek Authorization dla wszystkich kolejnych żądań
                // wysyłanych przez tę instancję _httpClient.
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Log.Debug("AuthService: Ustawiono nagłówek autoryzacji z tokenem.");
            }
            else
            {
                ClearAuthorizationHeader();
            }
        }

        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Log.Debug("AuthService: Usunięto nagłówek autoryzacji.");
        }



        public async Task<bool> LoginAsync(LoginRequestModel loginRequestModel)
        {
            string path = "/api/auth/login";
            string loginEndpointUrl = $"{ServerApiBaseUrl}{path}";
            Log.Debug("AuthService: Pełny URL do logowania: {Url}", loginEndpointUrl);

            try
            {
                // serializacja Jsona -> zbudowanie Jsona z dostępnych dannych
                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(loginRequestModel);


                /*Log.Debug("AuthService: Wysyłamy Json payload {Payload}", jsonPayload);

                // utworzenie ciała zapytania z Jasonem, kodowaniem  oraz ścieżką endpointa
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // wysyłaanie danych na wskazany endpoint
                Log.Information("AuthService: Wysyłanie żądania POST na adres: {Url}", loginEndpointUrl);
                HttpResponseMessage httpResponse = await _httpClient.PostAsync(loginEndpointUrl, httpContent);

                if (httpResponse != null)
                {
                    Log.Information("AuthService: Otrzymano odpowiedź od serwera ze statusem: {StatusCode}",
                        httpResponse.StatusCode);
                    return httpResponse.IsSuccessStatusCode;
                }
                Wykonane dla celów edukacyjnych :)
                */

                // ---Automatyczne serializacja JSONa za pomoca PostAsJsonAsync---
                Log.Information(
                    "AuthService: Wysyłanie żądania POST (używając PostAsJsonAsync) na adres: {Url} dla użytkownika {Username}",
                    loginEndpointUrl, loginRequestModel.Username);

                // Utwożenie responda. Do responda przypisywana jest od razu odpowiedź. Za wysyłanie odpowiada
                // metoda PostAsJsonAsync, w którym przekazujemy pełną ścieżke endpointa oraz dane logowania w formie
                // instancji LoginRequestModel. Await odpowiada za to, że aplikacja czeka na zakończenie operacji, 
                // jednocześnie nie blokując głównego wątku

                HttpResponseMessage httpResponse =
                    await _httpClient.PostAsJsonAsync(loginEndpointUrl, loginRequestModel);

                if (httpResponse.IsSuccessStatusCode)
                {
                    // --- Logowanie zakończone sukcesem po stronie klienta ---
                    Log.Information(
                        "AuthService: Logowanie przebiegło pomyślnie dla użytkownika {Username}. Status: {StatusCode}",
                        loginRequestModel.Username, httpResponse.StatusCode);

                    // Deserializacja odpowiedzi sukcesu z serwera
                    // Pamiętaj, że serwer ma zwracać obiekt LoginSuccessResponse

                    LoginSuccessResponse? loginResponse = null;
                    try
                    {
                        loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
                    }
                    catch (JsonException jsonEx)
                    {
                        Log.Error(jsonEx, "AuthService: Błąd deserializacji LoginSuccessResponse. Treść odpowiedzi: {Content}",
                                  await httpResponse.Content.ReadAsStringAsync());

                        // Jeśli nie uda się zdeserializować odpowiedzi sukcesu,
                        // traktujemy to jako błąd i nie zapisujemy sesji.
                        Log.Warning("AuthService: Nie udało się zdeserializować LoginSuccessResponse. Sprawdź, czy serwer zwraca poprawny format JSON.");
                        return false;
                    }

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {

                        _currentUserSessionService.SetUserSession(loginResponse);
                        Log.Information("AuthService: Dane sesji użytkownika dla {Username} zapisane.", loginResponse.Username);
                        return true;
                    }
                    else
                    {
                        Log.Warning("AuthService: Logowanie pomyślne (status {StatusCode}), ale token lub dane użytkownika są puste.",
                                    httpResponse.StatusCode);
                        return false; // Traktujemy jako błąd, jeśli nie ma tokena
                    }
                }
                else // Status odpowiedzi nie jest sukcesem (np. 400, 401, 403, 500)
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();

                    Log.Warning(
                        "AuthService: Logowanie nie powiodło się dla użytkownika: {Username}, Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        loginRequestModel.Username, httpResponse.StatusCode,
                        string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

                    // Próba deserializacji błędu, jeśli serwer zwraca błędy w formacie List<IdentityErrorResponse>
                    List<IdentityErrorResponse>? errors = null;
                    if (!string.IsNullOrEmpty(errorContent))
                        try
                        {
                            // Upewnij się, że masz using System.Collections.Generic; i using System.Linq;
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
                        // Tutaj możesz zdecydować, czy chcesz zwrócić te szczegółowe błędy do UI
                        // Obecnie metoda zwraca bool, więc po prostu logujemy.
                    }

                    return false; // Logowanie nieudane
                }
            }
            catch (HttpRequestException e) // Problemy z połączeniem, serwer nieosiągalny itp.
            {
                Log.Error(e,
                    "AuthService: Błąd żądania HTTP podczas logowania dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
                    loginRequestModel.Username, loginEndpointUrl);
                return false;
            }
            catch (JsonException e) // Błędy serializacji/deserializacji JSON
            {
                Log.Error(e, "AuthService: Błąd JSON podczas logowania dla {Username}. Problem z serializacją żądania lub deserializacją odpowiedzi.",
                    loginRequestModel.Username);
                return false;
            }
            catch (Exception e) // Inne, nieoczekiwane wyjątki
            {
                Log.Error(e, "AuthService: Nieoczekiwany błąd podczas logowania użytkownika {Username}",
                    loginRequestModel.Username);
                return false;
            }
        }
        public class RegistrationResultModel
        {
            public bool Succeeded { get; set; }
            public List<IdentityErrorResponse>? Errors { get; set; }
        }

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