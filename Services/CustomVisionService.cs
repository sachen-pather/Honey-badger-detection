using Honey_badger_detection.Configuration;
using Honey_badger_detection.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Honey_badger_detection.Services
{
    public class CustomVisionService : ICustomVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomVisionSettings _settings;
        private readonly ILogger<CustomVisionService> _logger;

        public CustomVisionService(
            HttpClient httpClient,
            IOptions<CustomVisionSettings> settings,
            ILogger<CustomVisionService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<CustomVisionResponse> AnalyzeImageAsync(IFormFile image)
        {
            try
            {
                // Reset headers before each request to avoid duplicates
                _httpClient.DefaultRequestHeaders.Clear();

                // Add Prediction-Key header
                _httpClient.DefaultRequestHeaders.Add("Prediction-Key", _settings.PredictionKey);

                // Log the request details for debugging
                _logger.LogInformation($"Sending request to endpoint: {_settings.Endpoint}");

                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                using var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Use a timeout to avoid hanging requests
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                var response = await _httpClient.PostAsync(_settings.Endpoint, content, cts.Token);
                response.EnsureSuccessStatusCode();

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = await response.Content.ReadFromJsonAsync<CustomVisionResponse>(jsonOptions, cts.Token)
                    ?? throw new Exception("Failed to deserialize response");

                _logger.LogInformation($"Received predictions: {result.Predictions.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CustomVisionService.AnalyzeImageAsync");
                throw;
            }
        }
    }
}