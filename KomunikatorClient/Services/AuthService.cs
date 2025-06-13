using System;
using System.Collections.Generic;
using System.Linq; // Upewnij się, że masz to dla SelectMany
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // <-- Upewnij się, że masz ten using dla PostAsJsonAsync
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using KomunikatorClient.DTOs;
using KomunikatorShared.DTOs; // Upewnij się, że to jest poprawny using dla DTOs

namespace KomunikatorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string ServerApiBaseUrl = "https://localhost:7233";
        private readonly CurrentUserSessionService _currentUserSessionService;

        public AuthService(CurrentUserSessionService currentUserSessionService, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _currentUserSessionService = currentUserSessionService;
        }

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
                // === ZMIANA TUTAJ: Używamy PostAsJsonAsync zamiast FormUrlEncodedContent ===
                Log.Information("AuthService: Wysyłanie żądania POST (JSON) na adres: {Url} dla użytkownika {Username}",
                    loginEndpointUrl, loginRequestModel.Username);

                HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(loginEndpointUrl, loginRequestModel);
                // === KONIEC ZMIANY ===

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
                        Log.Warning("AuthService: Logowanie pomyślne, ale token lub dane użytkownika są puste.");
                        return false;
                    }
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    Log.Warning("AuthService: Logowanie nie powiodło się dla użytkownika {Username}, Status: {StatusCode}, Treść błędu: {ErrorContent}",
                        loginRequestModel.Username, httpResponse.StatusCode, string.IsNullOrEmpty(errorContent) ? "[Brak]" : errorContent);

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