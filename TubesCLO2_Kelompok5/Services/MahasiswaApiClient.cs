using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using TubesCLO2_Kelompok5.Models;
using Microsoft.Extensions.Configuration;
using TubesCLO2_Kelompok5.Services;

namespace MahasiswaCLI.Services
{
    public class MahasiswaApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigurationService _configService;
        private readonly JsonSerializerOptions _jsonOptions;

        public MahasiswaApiClient(ConfigurationService configService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));
            _configService = configService;

            ArgumentException.ThrowIfNullOrWhiteSpace(_configService.ApiBaseUrl, nameof(_configService.ApiBaseUrl));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configService.ApiBaseUrl) 
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            Debug.Assert(_httpClient != null, "HttpClient should be initialized.");
            Debug.Assert(_httpClient.BaseAddress != null, "HttpClient BaseAddress should be set.");
        }

        private async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T data) where T : class
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestUri, nameof(requestUri));
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Debug.Assert(_httpClient != null, "HttpClient not initialized");

            try
            {
                return await _httpClient.PutAsJsonAsync(requestUri, data, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (PUT {requestUri}): {ex.Message}");
                return new HttpResponseMessage(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
            }
        }

        private async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestUri, nameof(requestUri));
            Debug.Assert(_httpClient != null, "HttpClient not initialized");

            try
            {
                return await _httpClient.DeleteAsync(requestUri);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (DELETE {requestUri}): {ex.Message}");
                return new HttpResponseMessage(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}