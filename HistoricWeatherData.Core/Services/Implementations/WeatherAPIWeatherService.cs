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
            var url = $"https://api.weatherapi.com/v1/history.json?key={apiKey}&q={parameters.Location.Latitude},{parameters.Location.Longitude}&dt={date:yyyy-MM-dd}";

            LoggingService.LogApiRequest($"{ProviderName}-{date:yyyy-MM-dd}", url, new Dictionary<string, string>
            {
                ["latitude"] = parameters.Location.Latitude.ToString(),
                ["longitude"] = parameters.Location.Longitude.ToString(),
                ["date"] = date.ToString("yyyy-MM-dd")
            });

            var requestStartTime = DateTime.Now;
            var response = await HttpClient.GetAsync(url);
            var requestDuration = DateTime.Now - requestStartTime;

            diagnostics.SetStatusCode((int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                diagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                LoggingService.LogApiResponse($"{ProviderName}-{date:yyyy-MM-dd}", (int)response.StatusCode, errorContent, requestDuration);
                LoggingService.LogError($"{ProviderName} API returned {(int)response.StatusCode} for date {date:yyyy-MM-dd}: {errorContent}");
                diagnostics.Complete(false, (int)response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new ApiAuthenticationException($"Authentication failed for {ProviderName}. Please check your API key.");
                }

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            LoggingService.LogApiResponse($"{ProviderName}-{date:yyyy-MM-dd}", (int)response.StatusCode, responseContent, requestDuration);

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
