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
                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(loginRequestModel);
                /*Log.Debug("AuthService: Wysyłamy Json payload {Payload}", jsonPayload);

                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

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
                    Log.Warning("AuthService: Logowanie nieudane dla użytkownika {Username}. Status: {StatusCode}",
                        loginRequestModel.Username, httpResponse.StatusCode);
                    
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
                Log.Error("AuthService: Błąd żądania http podczas logowania logowania dla {Username}. Serwer może być nieosiągalny lub wystąpił problem z siecią. Sprawdź adres: {Url}",
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
                ;
                return false;
            }
        }
    }
}