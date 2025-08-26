using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Net.Http.Json;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class ReverseGeocodingService : IReverseGeocodingService
    {
        private readonly HttpClient _httpClient;

        public ReverseGeocodingService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LocationData> GetLocationDataAsync(double latitude, double longitude)
        {
            try
            {
                // Using OpenStreetMap Nominatim API for reverse geocoding (free)
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<NominatimResponse>();

                if (result != null)
                {
                    return new LocationData
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        CityName = result.address?.city ?? result.address?.town ?? result.address?.village ?? result.display_name?.Split(',')[0] ?? "Unknown",
                        Country = result.address?.country ?? "Unknown",
                        State = result.address?.state ?? result.address?.region ?? "Unknown"
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error and return basic location data
                Console.WriteLine($"Reverse geocoding failed: {ex.Message}");
            }

            return new LocationData
            {
                Latitude = latitude,
                Longitude = longitude,
                CityName = $"{latitude:F2}, {longitude:F2}",
                Country = "Unknown",
                State = "Unknown"
            };
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