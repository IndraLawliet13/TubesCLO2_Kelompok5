using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using TubesCLO2_Kelompok5.Models;
namespace TubesCLO2_Kelompok5.Services
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

            if (string.IsNullOrWhiteSpace(_configService.ApiBaseUrl))
            {
                throw new ArgumentException("API Base URL is not configured.", nameof(_configService.ApiBaseUrl));
            }
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
        private async Task<T?> GetAsync<T>(string requestUri) where T : class
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestUri, nameof(requestUri));
            Debug.Assert(_httpClient != null, "HttpClient not initialized");
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode(); 
                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (GET {requestUri}): {ex.Message} (Status: {(ex.StatusCode.HasValue ? ex.StatusCode.Value.ToString() : "N/A")})");
                Debug.Assert(ex.StatusCode.HasValue && (int)ex.StatusCode.Value >= 400, "Exception should correspond to error status code.");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (GET {requestUri}): {ex.Message}");
                return null;
            }
        }
        private async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T data) where T : class
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestUri, nameof(requestUri));
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Debug.Assert(_httpClient != null, "HttpClient not initialized");
            try
            {
                return await _httpClient.PostAsJsonAsync(requestUri, data, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (POST {requestUri}): {ex.Message}");
                return new HttpResponseMessage(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
            }
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
        public async Task<IEnumerable<Mahasiswa>?> GetAllMahasiswaAsync(string? nim = null, string? nama = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(nim)) queryParams.Add($"nim={Uri.EscapeDataString(nim)}");
            if (!string.IsNullOrWhiteSpace(nama)) queryParams.Add($"nama={Uri.EscapeDataString(nama)}");
            string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            string requestUrl = $"api/mahasiswa{queryString}";
            _configService?.GetMessage("Searching");
            Console.WriteLine($"Calling API: GET {requestUrl}");
            return await GetAsync<List<Mahasiswa>>(requestUrl);
        }
        public async Task<System.Net.HttpStatusCode> UpdateMahasiswaAsync(string nim, Mahasiswa mhs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            ArgumentNullException.ThrowIfNull(mhs, nameof(mhs));
            Debug.Assert(nim.Equals(mhs.NIM, StringComparison.OrdinalIgnoreCase), "NIM in URL must match NIM in body for PUT.");
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: PUT {requestUrl}");
            HttpResponseMessage response = await PutAsync(requestUrl, mhs);
            return response.StatusCode;
        }
        public async Task<System.Net.HttpStatusCode> DeleteMahasiswaAsync(string nim)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            if (!Utils.InputValidator.IsValidNIM(nim))
            {
                _configService?.GetMessage("ErrorInvalidInput", $"Format NIM '{nim}' tidak valid."); 
                return System.Net.HttpStatusCode.BadRequest;
            }
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: DELETE {requestUrl}");
            HttpResponseMessage response = await DeleteAsync(requestUrl);
            return response.StatusCode;
        }
        public async Task<(bool Success, bool Conflict, Mahasiswa? CreatedMahasiswa)> AddMahasiswaAsync(Mahasiswa mhs)
        {
            ArgumentNullException.ThrowIfNull(mhs, nameof(mhs));
            string requestUrl = "api/mahasiswa";
            Console.WriteLine($"Calling API: POST {requestUrl}");
            HttpResponseMessage response = await PostAsync(requestUrl, mhs);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<Mahasiswa>(_jsonOptions);
                return (true, false, created);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return (false, true, null);
            }
            return (false, false, null);
        }
        public async Task<Mahasiswa?> GetMahasiswaByNIMAsync(string nim)
        {
            // DbC: Precondition
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            if (!Utils.InputValidator.IsValidNIM(nim))
            {
                Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", $"Format NIM '{nim}' tidak valid."));
                return null;
            }
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: GET {requestUrl}");
            return await GetAsync<Mahasiswa>(requestUrl);
        }
    }
}
