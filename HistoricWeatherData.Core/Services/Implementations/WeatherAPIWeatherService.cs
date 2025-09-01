using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class WeatherAPIWeatherService : BaseWeatherDataService
    {
        public override string ProviderName => "WeatherAPI";
        public override bool RequiresApiKey => true;

        public WeatherAPIWeatherService(ILoggingService loggingService, ISettingsService settingsService, HttpClient httpClient)
            : base(loggingService, settingsService, httpClient)
        {
        }

        public WeatherAPIWeatherService(ILoggingService loggingService, ISettingsService settingsService)
            : base(loggingService, settingsService, new HttpClient { Timeout = TimeSpan.FromSeconds(300) })
        {
        }

        protected override async Task<WeatherData?> FetchDayDataAsync(WeatherQueryParameters parameters, DateTime date, string? apiKey, ApiDiagnostics diagnostics)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                LoggingService.LogError($"WeatherAPI requires an API key but none was provided for date {date:yyyy-MM-dd}");
                return null;
            }

            var url = $"https://api.weatherapi.com/v1/history.json?key={apiKey}&q={parameters.Location.Latitude},{parameters.Location.Longitude}&dt={date:yyyy-MM-dd}";

            LoggingService.LogInformation($"[{ProviderName}] Starting API request for {date:yyyy-MM-dd} - Lat: {parameters.Location.Latitude:F4}, Lon: {parameters.Location.Longitude:F4}");

            var requestStartTime = DateTime.Now;
            var response = await HttpClient.GetAsync(url);
            var requestDuration = DateTime.Now - requestStartTime;

            diagnostics.SetStatusCode((int)response.StatusCode);

            LoggingService.LogInformation($"[{ProviderName}] API Response Status: {(int)response.StatusCode} ({GetHttpStatusText((int)response.StatusCode)}) - Duration: {requestDuration.TotalMilliseconds:F0}ms");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LoggingService.LogError($"[{ProviderName}] API Error Response - Status: {(int)response.StatusCode}, Content: {errorContent}");
                diagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                LoggingService.LogApiError($"{ProviderName}-{date:yyyy-MM-dd}", url, new Exception(errorContent), requestDuration);
                diagnostics.Complete(false, (int)response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new ApiAuthenticationException($"Authentication failed for {ProviderName}. Please check your API key.");
                }

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            LoggingService.LogInformation($"[{ProviderName}] Raw API Response Length: {responseContent.Length} characters");

            WeatherAPIResponse? result;
            try
            {
                result = await response.Content.ReadFromJsonAsync<WeatherAPIResponse>();
                LoggingService.LogInformation($"[{ProviderName}] JSON deserialization successful for {date:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"[{ProviderName}] JSON deserialization failed for {date:yyyy-MM-dd}: {ex.Message}\nRaw response: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");
                throw;
            }

            if (result?.forecast?.forecastday == null || !result.forecast.forecastday.Any())
            {
                LoggingService.LogWarning($"[{ProviderName}] No forecast data available in API response for {date:yyyy-MM-dd}. Response contains: forecast={result?.forecast != null}, forecastday={(result?.forecast?.forecastday?.Length ?? 0)} items");
                return null;
            }

            var forecastDays = result.forecast.forecastday;
            if (forecastDays.Length == 0)
            {
                LoggingService.LogWarning($"[{ProviderName}] Empty forecastday array in response for {date:yyyy-MM-dd}");
                return null;
            }

            var forecastDay = forecastDays[0];
            var day = forecastDay.day;

            if (day == null)
            {
                LoggingService.LogWarning($"[{ProviderName}] Day object is null in forecastday[0] for {date:yyyy-MM-dd}. Available properties: {string.Join(", ", typeof(ForecastDay).GetProperties().Select(p => p.Name))}");
                return null;
            }

            var weatherData = new WeatherData
            {
                Date = date,
                TemperatureMin = day.mintemp_c,
                TemperatureMax = day.maxtemp_c,
                Precipitation = day.totalprecip_mm,
                WeatherProvider = ProviderName,
                Location = parameters.Location
            };

            LoggingService.LogInformation($"[{ProviderName}] Successfully parsed weather data for {date:yyyy-MM-dd}: Min={weatherData.TemperatureMin:F1}°C, Max={weatherData.TemperatureMax:F1}°C, Precipitation={weatherData.Precipitation:F2}mm");

            diagnostics.Complete(true);
            return weatherData;
        }

        private string GetHttpStatusText(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                _ => $"Status {statusCode}"
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
