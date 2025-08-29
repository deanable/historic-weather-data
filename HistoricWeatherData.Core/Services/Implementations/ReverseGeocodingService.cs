using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Net.Http.Json;
using System.Diagnostics;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class ReverseGeocodingService : IReverseGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;

        public ReverseGeocodingService()
        {
            _httpClient = new HttpClient();
            // Add User-Agent header to comply with Nominatim terms of service
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HistoricWeatherData/1.0 (deanable@github.com)");
            _loggingService = new CompositeLoggingService(); // File + Console logging
        }

        public ReverseGeocodingService(ILoggingService loggingService)
        {
            _httpClient = new HttpClient();
            // Add User-Agent header to comply with Nominatim terms of service
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HistoricWeatherData/1.0 (deanable@github.com)");
            _loggingService = loggingService;
        }

        public async Task<LocationData> GetLocationDataAsync(double latitude, double longitude)
        {
            var diagnostics = new ApiDiagnostics("ReverseGeocodingService", "GetLocationData");

            try
            {
                diagnostics.AddRequest();

                // Using OpenStreetMap Nominatim API for reverse geocoding (free)
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1";

                _loggingService.LogApiRequest("NominatimReverseGeocoding", url, new Dictionary<string, string>
                {
                    ["lat"] = latitude.ToString(),
                    ["lon"] = longitude.ToString(),
                    ["zoom"] = "10",
                    ["format"] = "json",
                    ["addressdetails"] = "1"
                });

                var startTime = DateTime.Now;
                var response = await _httpClient.GetAsync(url);
                var duration = DateTime.Now - startTime;

                diagnostics.SetStatusCode((int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    diagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                    _loggingService.LogApiResponse("NominatimReverseGeocoding", (int)response.StatusCode, errorContent, duration);
                    _loggingService.LogError($"Nominatim API returned {response.StatusCode}: {errorContent}");
                    diagnostics.Complete(false, (int)response.StatusCode);
                    return CreateFallbackLocation(latitude, longitude, "Nominatim API error");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _loggingService.LogApiResponse("NominatimReverseGeocoding", (int)response.StatusCode, responseContent, duration);

                var result = await response.Content.ReadFromJsonAsync<NominatimResponse>();

                if (result != null)
                {
                    var location = new LocationData
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        CityName = result.address?.city ?? result.address?.town ?? result.address?.village ?? result.display_name?.Split(',')[0] ?? "Unknown",
                        Country = result.address?.country ?? "Unknown",
                        State = result.address?.state ?? result.address?.region ?? "Unknown"
                    };

                    diagnostics.Complete(true, (int)response.StatusCode);
                    _loggingService.LogInformation($"{diagnostics.GetSummary()} - Resolved to: {location.CityName}, {location.Country}");
                    return location;
                }
                else
                {
                    diagnostics.AddError("Nominatim API returned null response");
                    diagnostics.Complete(false, (int)response.StatusCode);
                    _loggingService.LogWarning($"Nominatim API returned null result for coordinates: {latitude}, {longitude}");
                }
            }
            catch (HttpRequestException ex)
            {
                diagnostics.AddError($"Network error: {ex.Message}");
                diagnostics.Complete(false);
                _loggingService.LogApiError("NominatimReverseGeocoding", $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1", ex, diagnostics.Duration ?? TimeSpan.Zero);
                return CreateFallbackLocation(latitude, longitude, ex.Message);
            }
            catch (Exception ex)
            {
                diagnostics.AddError($"Unexpected error: {ex.Message}");
                diagnostics.Complete(false);
                _loggingService.LogError($"Unexpected error in reverse geocoding", ex);
                return CreateFallbackLocation(latitude, longitude, ex.Message);
            }

            diagnostics.Complete(false);
            _loggingService.LogInformation(diagnostics.GetSummary());
            return CreateFallbackLocation(latitude, longitude, "Unknown location");
        }

        private LocationData CreateFallbackLocation(double latitude, double longitude, string errorReason = null)
        {
            var location = new LocationData
            {
                Latitude = latitude,
                Longitude = longitude,
                CityName = $"{latitude:F4}, {longitude:F4}",
                Country = "Unknown",
                State = "Unknown"
            };

            _loggingService.LogWarning($"Using fallback location data for {location.CityName}. Reason: {errorReason ?? "No address found"}");
            return location;
        }

        private class NominatimResponse
        {
            public string? display_name { get; set; }
            public Address? address { get; set; }
        }

        private class Address
        {
            public string? city { get; set; }
            public string? town { get; set; }
            public string? village { get; set; }
            public string? state { get; set; }
            public string? region { get; set; }
            public string? country { get; set; }
        }
    }
}