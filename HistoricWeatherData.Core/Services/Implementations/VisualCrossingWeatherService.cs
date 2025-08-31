using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Net.Http.Json;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class VisualCrossingWeatherService : IWeatherDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;

        public string ProviderName => "Visual Crossing";
        public bool RequiresApiKey => true;

        public VisualCrossingWeatherService(ILoggingService loggingService, ISettingsService settingsService)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(300);
            _loggingService = loggingService;
            _settingsService = settingsService;
        }

        public async Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters)
        {
            var apiKey = await _settingsService.GetApiKeyAsync(ProviderName);
            if (string.IsNullOrEmpty(apiKey))
            {
                return new WeatherResponse { IsSuccess = false, ErrorMessage = $"API key for {ProviderName} is not set." };
            }

            var diagnostics = new ApiDiagnostics("VisualCrossingWeatherService", "GetHistoricalWeatherData");

            try
            {
                diagnostics.AddRequest();
                _loggingService.LogInformation($"Starting weather data request for location ({parameters.Location.Latitude:F4}, {parameters.Location.Longitude:F4}) - {parameters.YearsBack} years back");

                var allWeatherData = new List<WeatherData>();
                var failedYears = new List<int>();
                var successfulYears = 0;

                var currentYear = DateTime.Now.Year;
                var startYear = currentYear - parameters.YearsBack;

                for (int year = startYear; year <= currentYear; year++)
                {
                    try
                    {
                        var yearlyData = await GetWeatherDataForYearAsync(parameters, year, apiKey, diagnostics);
                        if (yearlyData.Any())
                        {
                            allWeatherData.AddRange(yearlyData);
                            successfulYears++;
                        }
                        else
                        {
                            failedYears.Add(year);
                        }
                    }
                    catch (Exception yearEx)
                    {
                        failedYears.Add(year);
                        diagnostics.AddError($"Year {year}: {yearEx.Message}");
                        _loggingService.LogError($"Failed to retrieve data for year {year}", yearEx);
                    }
                }

                var response = new WeatherResponse
                {
                    Data = allWeatherData,
                    IsSuccess = true
                };

                diagnostics.Complete(true);

                var summary = $"Completed weather data request: {allWeatherData.Count} total records from {successfulYears} years";
                if (failedYears.Any())
                {
                    summary += $", {failedYears.Count} failed years: {string.Join(", ", failedYears)}";
                }

                _loggingService.LogInformation(summary);
                return response;
            }
            catch (Exception ex)
            {
                diagnostics.Complete(false);
                _loggingService.LogError("Critical error in weather data retrieval", ex);
                _loggingService.LogInformation(diagnostics.GetSummary());

                return new WeatherResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"{ProviderName} API error: {ex.Message}. See logs for details."
                };
            }
        }

        private async Task<List<WeatherData>> GetWeatherDataForYearAsync(WeatherQueryParameters parameters, int year, string apiKey, ApiDiagnostics diagnostics)
        {
            var yearStartDate = new DateTime(year, parameters.StartDate.Month, parameters.StartDate.Day);
            var yearEndDate = new DateTime(year, (parameters.EndDate ?? parameters.StartDate.AddDays(1)).Month, (parameters.EndDate ?? parameters.StartDate.AddDays(1)).Day);

            var formattedLatitude = parameters.Location.Latitude.ToString("F6").Replace(',', '.');
            var formattedLongitude = parameters.Location.Longitude.ToString("F6").Replace(',', '.');

            var url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{formattedLatitude},{formattedLongitude}/{yearStartDate:yyyy-MM-dd}/{yearEndDate:yyyy-MM-dd}?unitGroup=metric&key={apiKey}&include=days";

            _loggingService.LogApiRequest($"{ProviderName}-Year{year}", url, new Dictionary<string, string>
            {
                ["latitude"] = formattedLatitude,
                ["longitude"] = formattedLongitude,
                ["start_date"] = yearStartDate.ToString("yyyy-MM-dd"),
                ["end_date"] = yearEndDate.ToString("yyyy-MM-dd")
            });

            var requestStartTime = DateTime.Now;
            var response = await _httpClient.GetAsync(url);
            var requestDuration = DateTime.Now - requestStartTime;

            diagnostics.SetStatusCode((int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                diagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                _loggingService.LogApiResponse($"{ProviderName}-Year{year}", (int)response.StatusCode, errorContent, requestDuration);
                _loggingService.LogError($"{ProviderName} API returned {(int)response.StatusCode} for year {year}: {errorContent}");
                diagnostics.Complete(false, (int)response.StatusCode);
                return new List<WeatherData>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _loggingService.LogApiResponse($"{ProviderName}-Year{year}", (int)response.StatusCode, responseContent, requestDuration);

            var result = await response.Content.ReadFromJsonAsync<VisualCrossingResponse>();

            var weatherData = new List<WeatherData>();
            if (result?.days != null)
            {
                foreach (var day in result.days)
                {
                    weatherData.Add(new WeatherData
                    {
                        Date = DateTime.Parse(day.datetime),
                        TemperatureMin = day.tempmin,
                        TemperatureMax = day.tempmax,
                        Precipitation = day.precip,
                        WeatherProvider = ProviderName,
                        Location = parameters.Location
                    });
                }
            }

            return weatherData;
        }

        private class VisualCrossingResponse
        {
            public Day[]? days { get; set; }
        }

        private class Day
        {
            public string datetime { get; set; } = string.Empty;
            public double tempmax { get; set; }
            public double tempmin { get; set; }
            public double precip { get; set; }
        }
    }
}
