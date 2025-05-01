using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace TubesCLO2_Kelompok5.Services
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultLanguage;
        private readonly Dictionary<string, string> _messages;

        public string ApiBaseUrl { get; }

        public ConfigurationService()
        {
            // Setup configuration provider
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Cari appsettings.json di direktori output
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // DbC: Pastikan konfigurasi ter-load
            Debug.Assert(_configuration != null, "Configuration should not be null after build.");

            // Ambil URL API dari config
            ApiBaseUrl = _configuration.GetValue<string>("ApiConfig:BaseUrl") ?? "http://localhost:5000"; // DbC: Pastikan URL tidak kosong
            ArgumentException.ThrowIfNullOrWhiteSpace(ApiBaseUrl, nameof(ApiBaseUrl));
            Console.WriteLine($"API Base URL set to: {ApiBaseUrl}"); // Debugging info

            // Ambil bahasa default
            _defaultLanguage = _configuration.GetValue<string>("AppConfig:DefaultLanguage") ?? "id";

            // Load messages untuk bahasa default
            _messages = _configuration.GetSection($"AppConfig:Messages:{_defaultLanguage}")
                                      .Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            Debug.Assert(_messages != null, $"Messages for language '{_defaultLanguage}' could not be loaded.");
        }

        public string GetMessage(string key, params object[] args)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            if (_messages.TryGetValue(key, out var messageFormat))
            {
                return args.Length > 0 ? string.Format(messageFormat, args) : messageFormat;
            }

            Debug.WriteLine($"Warning: Message key '{key}' not found for language '{_defaultLanguage}'.");
            return $"[{key}]";
        }
    }
}
