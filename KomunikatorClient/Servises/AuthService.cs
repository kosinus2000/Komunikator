using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KomunikatorClient.Models;
using Serilog;
using System.Net.Http.Json;

namespace KomunikatorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string ServerApiBaseUrl = "https://localhost:7233";

        public AuthService()
        {
            _httpClient = new HttpClient();
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
                    Log.Information(
                        "AuthService: Logowanie przebiegło pomyślnie dla użytkownika {Username}. Status: {StatusCode}",
                        loginRequestModel.Username, httpResponse.StatusCode);
                    return true;
                }
                else
                {
                    // W tym przypadku następuje utworzenie stringa z błędem. Dzięki await aplikacja czeka na wysłanie 
                    // błegu. HttpResponse.Content pobiera treść odpowiedzi HTTP nataomiast .ReadAsStringAsync()
                    // przekształca zawartość odpowiedzi na tekst asynchronicznie.
                    // Muszę pamiętać, że odpowiedź z serwera orginalnie ma format JSONa np.
                    // { "message": "Niepoprawne dane logowania", "errorCode": 401 }
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();

                    Log.Warning(
                        "AuthService: Logowanie nie powiodło się dla użytkownika {Username}. Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        loginRequestModel.Username, httpResponse.StatusCode,
                        // jezeli string jest pusty to zwraca [Brak], jeżeli nie, wypisuje errorContent
                        string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

                    //deserializacja błędu
                    ErrorResponse? structuredError = null;
                    if (!string.IsNullOrEmpty(errorContent))
                        try
                        {
                            structuredError = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                        }
                        catch (JsonException eJson)
                        {
                            Log.Error(eJson,
                                "AuthService: Nie udało sie zdeserializować treści błędu jako ErrorResponse. Treść {ErrorContent}",
                                errorContent);
                        }

                    if (structuredError != null && !string.IsNullOrEmpty(structuredError.Message))
                    {
                        Log.Warning("AuthService: ");
                    }

                    return false;
                }
                // if (httpResponse != null)
                // {
                //     Log.Information("AuthService: Otrzymano odpowiedź od serwera (PostAsJsonAsync) ze statusem: {StatusCode}", httpResponse.StatusCode);
                //     return httpResponse.IsSuccessStatusCode;
                // }

                return false;
            }
            catch (HttpRequestException e)
            {
                Log.Error(
                    "AuthService: Błąd żądania http podczas logowania logowania dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
                    loginRequestModel.Username, loginEndpointUrl);
                return false;
            }
            catch (JsonException e)
            {
                Log.Error("AuthService: Błąd wysłanego json podczas przygotowania żądania dla {Username}",
                    loginRequestModel.Username);
                return false;
            }
            catch (Exception e)
            {
                Log.Error("AuthService: Nieoczekiwany bład podczas logowania użytkownika {Username}",
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