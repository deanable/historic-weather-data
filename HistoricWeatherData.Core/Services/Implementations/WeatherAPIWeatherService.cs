using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class WeatherAPIWeatherService : IWeatherDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;

        public string ProviderName => "WeatherAPI";
        public bool RequiresApiKey => true;

        public WeatherAPIWeatherService(ILoggingService loggingService, ISettingsService settingsService)
            : this(loggingService, settingsService, new HttpClient { Timeout = TimeSpan.FromSeconds(300) })
        {
        }

        public WeatherAPIWeatherService(ILoggingService loggingService, ISettingsService settingsService, HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            var diagnostics = new ApiDiagnostics("WeatherAPIWeatherService", "GetHistoricalWeatherData");

            try
            {
                diagnostics.AddRequest();
                _loggingService.LogInformation($"Starting weather data request for location ({parameters.Location.Latitude:F4}, {parameters.Location.Longitude:F4}) - {parameters.YearsBack} years back");

                var allWeatherData = new List<WeatherData>();
                var failedDates = new List<DateTime>();
                var successfulDates = 0;

                var startDate = parameters.StartDate;
                var endDate = parameters.EndDate ?? DateTime.Now;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    try
                    {
                        var dailyData = await GetWeatherDataForDateAsync(parameters, date, apiKey, diagnostics);
                        if (dailyData != null)
                        {
                            allWeatherData.Add(dailyData);
                            successfulDates++;
                        }
                        else
                        {
                            failedDates.Add(date);
                        }
                    }
                    catch (Exception dateEx)
                    {
                        failedDates.Add(date);
                        diagnostics.AddError($"Date {date:yyyy-MM-dd}: {dateEx.Message}");
                        _loggingService.LogError($"Failed to retrieve data for date {date:yyyy-MM-dd}", dateEx);
                    }
                }

                var response = new WeatherResponse
                {
                    Data = allWeatherData,
                    IsSuccess = true
                };

                diagnostics.Complete(true);

                var summary = $"Completed weather data request: {allWeatherData.Count} total records from {successfulDates} dates";
                if (failedDates.Any())
                {
                    summary += $", {failedDates.Count} failed dates";
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

        private async Task<WeatherData?> GetWeatherDataForDateAsync(WeatherQueryParameters parameters, DateTime date, string apiKey, ApiDiagnostics diagnostics)
        {
            var url = $"https://api.weatherapi.com/v1/history.json?key={apiKey}&q={parameters.Location.Latitude},{parameters.Location.Longitude}&dt={date:yyyy-MM-dd}";

            _loggingService.LogApiRequest($"{ProviderName}-{date:yyyy-MM-dd}", url, new Dictionary<string, string>
            {
                ["latitude"] = parameters.Location.Latitude.ToString(),
                ["longitude"] = parameters.Location.Longitude.ToString(),
                ["date"] = date.ToString("yyyy-MM-dd")
            });

            var requestStartTime = DateTime.Now;
            var response = await _httpClient.GetAsync(url);
            var requestDuration = DateTime.Now - requestStartTime;

            diagnostics.SetStatusCode((int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                diagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                _loggingService.LogApiResponse($"{ProviderName}-{date:yyyy-MM-dd}", (int)response.StatusCode, errorContent, requestDuration);
                _loggingService.LogError($"{ProviderName} API returned {(int)response.StatusCode} for date {date:yyyy-MM-dd}: {errorContent}");
                diagnostics.Complete(false, (int)response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _loggingService.LogApiResponse($"{ProviderName}-{date:yyyy-MM-dd}", (int)response.StatusCode, responseContent, requestDuration);

            var result = await response.Content.ReadFromJsonAsync<WeatherAPIResponse>();

            if (result?.forecast?.forecastday == null || !result.forecast.forecastday.Any())
            {
                return null;
            }

            var day = result.forecast.forecastday[0].day;

            return new WeatherData
            {
                Date = date,
                TemperatureMin = day.mintemp_c,
                TemperatureMax = day.maxtemp_c,
                Precipitation = day.totalprecip_mm,
                WeatherProvider = ProviderName,
                Location = parameters.Location
            };
        }

        private class WeatherAPIResponse
        {
            public Forecast? forecast { get; set; }
        }

        private class Forecast
        {
            public ForecastDay[]? forecastday { get; set; }
        }

        private class ForecastDay
        {
            public Day? day { get; set; }
        }

        private class Day
        {
            public double maxtemp_c { get; set; }
            public double mintemp_c { get; set; }
            public double totalprecip_mm { get; set; }
        }
    }
}
