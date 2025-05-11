using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KomunikatorClient.Models;
using Serilog;

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
                Log.Debug("AuthService: Wysyłamy Json payload {Payload}",jsonPayload);
                
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                Log.Information("AuthService: Wysyłanie żądania POST na adres: {Url}", loginEndpointUrl);
                HttpResponseMessage httpResponse = await _httpClient.PostAsync(loginEndpointUrl, httpContent);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}