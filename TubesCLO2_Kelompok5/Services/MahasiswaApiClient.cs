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
        public async Task<ApiResponse<List<Mahasiswa>>?> GetAllMahasiswaAsync(string? nim = null, string? nama = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(nim)) queryParams.Add($"nim={Uri.EscapeDataString(nim)}");
            if (!string.IsNullOrWhiteSpace(nama)) queryParams.Add($"nama={Uri.EscapeDataString(nama)}");
            string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            string requestUrl = $"api/mahasiswa{queryString}";
            _configService?.GetMessage("Searching");
            Console.WriteLine($"Calling API: GET {requestUrl}");

            try
            {
                HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(requestUrl);
                string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Mahasiswa>>>(jsonResponse, _jsonOptions);
                if (apiResponse != null)
                {
                    Console.WriteLine($"API Message: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Gagal memproses respons dari API (GET {requestUrl}). Respons tidak valid.");
                    return null;
                }
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (GET {requestUrl}): {ex.Message} (Status: {(ex.StatusCode.HasValue ? ex.StatusCode.Value.ToString() : "N/A")})");
                return new ApiResponse<List<Mahasiswa>> { Status = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), Message = ex.Message, Data = null };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (GET {requestUrl}): {ex.Message}");
                return new ApiResponse<List<Mahasiswa>> { Status = 500, Message = $"Kesalahan format JSON: {ex.Message}", Data = null };
            }
            catch (Exception ex) // Catch-all untuk error tak terduga
            {
                Console.WriteLine($"Unexpected error (GET {requestUrl}): {ex.Message}");
                return new ApiResponse<List<Mahasiswa>> { Status = 500, Message = $"Kesalahan tidak terduga: {ex.Message}", Data = null };
            }
        }
        public async Task<ApiResponse<Mahasiswa>?> UpdateMahasiswaAsync(string nim, Mahasiswa mhs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            ArgumentNullException.ThrowIfNull(mhs, nameof(mhs));
            Debug.Assert(nim.Equals(mhs.NIM, StringComparison.OrdinalIgnoreCase), "NIM in URL must match NIM in body for PUT.");
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: PUT {requestUrl}");
            try
            {
                HttpResponseMessage httpResponseMessage = await PutAsync(requestUrl, mhs);
                string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Mahasiswa>>(jsonResponse, _jsonOptions);
                if (apiResponse != null)
                {
                    Console.WriteLine($"API Message: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Gagal memproses respons dari API (PUT {requestUrl}). Respons tidak valid.");
                    return null;
                }
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (PUT {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), Message = ex.Message, Data = null };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (PUT {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan format JSON: {ex.Message}", Data = null };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error (PUT {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan tidak terduga: {ex.Message}", Data = null };
            }
        }
        public async Task<ApiResponse<object>?> DeleteMahasiswaAsync(string nim)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            if (!Utils.InputValidator.IsValidNIM(nim))
            {
                string invalidMsg = _configService.GetMessage("ErrorInvalidInput", $"Format NIM '{nim}' tidak valid.");
                Console.WriteLine(invalidMsg);
                return new ApiResponse<object> { Status = 400, Message = invalidMsg, Data = null };
            }
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: DELETE {requestUrl}");
            try
            {
                HttpResponseMessage httpResponseMessage = await DeleteAsync(requestUrl);
                string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(jsonResponse, _jsonOptions);

                if (apiResponse != null)
                {
                    Console.WriteLine($"API Message: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Gagal memproses respons dari API (DELETE {requestUrl}). Respons tidak valid.");
                    return null;
                }
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (DELETE {requestUrl}): {ex.Message}");
                return new ApiResponse<object> { Status = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), Message = ex.Message, Data = null };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (DELETE {requestUrl}): {ex.Message}");
                return new ApiResponse<object> { Status = 500, Message = $"Kesalahan format JSON: {ex.Message}", Data = null };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error (DELETE {requestUrl}): {ex.Message}");
                return new ApiResponse<object> { Status = 500, Message = $"Kesalahan tidak terduga: {ex.Message}", Data = null };
            }
        }
        public async Task<ApiResponse<Mahasiswa>?> AddMahasiswaAsync(Mahasiswa mhs)
        {
            ArgumentNullException.ThrowIfNull(mhs, nameof(mhs));
            string requestUrl = "api/mahasiswa";
            Console.WriteLine($"Calling API: POST {requestUrl}");
            try
            {
                HttpResponseMessage httpResponseMessage = await PostAsync(requestUrl, mhs);
                string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Mahasiswa>>(jsonResponse, _jsonOptions);

                if (apiResponse != null)
                {
                    Console.WriteLine($"API Message: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Gagal memproses respons dari API (POST {requestUrl}). Respons tidak valid.");
                    return null;
                }
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (POST {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), Message = ex.Message, Data = null };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (POST {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan format JSON: {ex.Message}", Data = null };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error (POST {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan tidak terduga: {ex.Message}", Data = null };
            }
        }
        public async Task<ApiResponse<Mahasiswa>?> GetMahasiswaByNIMAsync(string nim)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nim, nameof(nim));
            if (!Utils.InputValidator.IsValidNIM(nim))
            {
                string invalidMsg = _configService.GetMessage("ErrorInvalidInput", $"Format NIM '{nim}' tidak valid.");
                Console.WriteLine(invalidMsg);
                return new ApiResponse<Mahasiswa> { Status = 400, Message = invalidMsg, Data = null };
            }
            string requestUrl = $"api/mahasiswa/{Uri.EscapeDataString(nim)}";
            Console.WriteLine($"Calling API: GET {requestUrl}");
            try
            {
                HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(requestUrl);
                string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Mahasiswa>>(jsonResponse, _jsonOptions);
                if (apiResponse != null)
                {
                    Console.WriteLine($"API Message: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Gagal memproses respons dari API (GET {requestUrl}). Respons tidak valid.");
                    return null;
                }
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error (GET {requestUrl}): {ex.Message} (Status: {(ex.StatusCode.HasValue ? ex.StatusCode.Value.ToString() : "N/A")})");
                return new ApiResponse<Mahasiswa> { Status = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), Message = ex.Message, Data = null };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error (GET {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan format JSON: {ex.Message}", Data = null };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error (GET {requestUrl}): {ex.Message}");
                return new ApiResponse<Mahasiswa> { Status = 500, Message = $"Kesalahan tidak terduga: {ex.Message}", Data = null };
            }
        }
    }
}
