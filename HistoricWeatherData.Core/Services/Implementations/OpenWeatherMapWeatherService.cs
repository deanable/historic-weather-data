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
    public class OpenWeatherMapWeatherService : BaseWeatherDataService
    {
        public override string ProviderName => "OpenWeatherMap";
        public override bool RequiresApiKey => true;

        public OpenWeatherMapWeatherService(ILoggingService loggingService, ISettingsService settingsService, HttpClient httpClient)
            : base(loggingService, settingsService, httpClient)
        {
        }

        public OpenWeatherMapWeatherService(ILoggingService loggingService, ISettingsService settingsService)
            : base(loggingService, settingsService, new HttpClient { Timeout = TimeSpan.FromSeconds(300) })
        {
        }

        protected override async Task<WeatherData?> FetchDayDataAsync(WeatherQueryParameters parameters, DateTime date, string? apiKey, ApiDiagnostics diagnostics)
        {
            var unixTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();

            var url = $"https://api.openweathermap.org/data/3.0/onecall/timemachine?lat={parameters.Location.Latitude}&lon={parameters.Location.Longitude}&dt={unixTimestamp}&appid={apiKey}&units=metric";

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

            var result = await response.Content.ReadFromJsonAsync<OpenWeatherMapResponse>();

            if (result?.data == null || !result.data.Any())
            {
                return null;
            }

            // Aggregate hourly data to daily
            var temps = result.data.Select(h => h.temp).ToList();
            var precip = result.data.Sum(h => h.rain?.h ?? 0) + result.data.Sum(h => h.snow?.h ?? 0);

            return new WeatherData
            {
                Date = date,
                TemperatureMin = temps.Min(),
                TemperatureMax = temps.Max(),
                Precipitation = precip,
                WeatherProvider = ProviderName,
                Location = parameters.Location
            };
        }

        private class OpenWeatherMapResponse
        {
            public HourlyData[]? data { get; set; }
        }

        private class HourlyData
        {
            public double temp { get; set; }
            public Rain? rain { get; set; }
            public Snow? snow { get; set; }
        }

        private class Rain
        {
            public double h { get; set; } // 1h precipitation
        }

        private class Snow
        {
            public double h { get; set; }
        }
    }
}
