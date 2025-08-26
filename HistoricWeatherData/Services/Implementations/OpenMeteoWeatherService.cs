using HistoricWeatherData.Models;
using HistoricWeatherData.Services.Interfaces;
using System.Net.Http.Json;

namespace HistoricWeatherData.Services.Implementations
{
    public class OpenMeteoWeatherService : IWeatherDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IReverseGeocodingService _geocodingService;

        public string ProviderName => "Open-Meteo";
        public bool RequiresApiKey => false;

        public OpenMeteoWeatherService(IReverseGeocodingService geocodingService)
        {
            _httpClient = new HttpClient();
            _geocodingService = geocodingService;
        }

        public async Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters)
        {
            try
            {
                var locationData = await _geocodingService.GetLocationDataAsync(
                    parameters.Location.Latitude,
                    parameters.Location.Longitude);

                var endDate = parameters.EndDate ?? parameters.StartDate.AddDays(1);

                // Calculate date range for historical data
                var currentYear = DateTime.Now.Year;
                var startYear = currentYear - parameters.YearsBack;

                var allWeatherData = new List<WeatherData>();

                for (int year = startYear; year <= currentYear; year++)
                {
                    var yearlyData = await GetWeatherDataForYearAsync(
                        parameters.Location.Latitude,
                        parameters.Location.Longitude,
                        parameters.StartDate,
                        endDate,
                        year,
                        locationData);

                    allWeatherData.AddRange(yearlyData);
                }

                return new WeatherResponse
                {
                    Data = allWeatherData,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new WeatherResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Open-Meteo API error: {ex.Message}"
                };
            }
        }

        private async Task<List<WeatherData>> GetWeatherDataForYearAsync(
            double latitude, double longitude, DateTime startDate, DateTime endDate, int year, LocationData locationData)
        {
            // Adjust dates for the specific year
            var yearStartDate = new DateTime(year, startDate.Month, startDate.Day);
            var yearEndDate = new DateTime(year, endDate.Month, endDate.Day);

            // If the target year is the current year and the date range is in the future, skip
            if (year == DateTime.Now.Year && yearStartDate > DateTime.Now)
            {
                return new List<WeatherData>();
            }

            var url = $"https://archive-api.open-meteo.com/v1/archive?" +
                     $"latitude={latitude}&longitude={longitude}" +
                     $"&start_date={yearStartDate:yyyy-MM-dd}&end_date={yearEndDate:yyyy-MM-dd}" +
                     $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
                     $"&timezone=auto";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>();

            if (result?.daily?.time == null || result.daily.temperature_2m_min == null || result.daily.temperature_2m_max == null || result.daily.precipitation_sum == null)
            {
                return new List<WeatherData>();
            }

            var weatherData = new List<WeatherData>();
            for (int i = 0; i < result.daily.time.Length; i++)
            {
                weatherData.Add(new WeatherData
                {
                    Date = DateTime.Parse(result.daily.time[i]),
                    TemperatureMin = result.daily.temperature_2m_min[i],
                    TemperatureMax = result.daily.temperature_2m_max[i],
                    Precipitation = result.daily.precipitation_sum[i],
                    WeatherProvider = ProviderName,
                    Location = locationData
                });
            }

            return weatherData;
        }

        private class OpenMeteoResponse
        {
            public DailyData? daily { get; set; }
        }

        private class DailyData
        {
            public string[]? time { get; set; }
            public double[]? temperature_2m_min { get; set; }
            public double[]? temperature_2m_max { get; set; }
            public double[]? precipitation_sum { get; set; }
        }
    }
}