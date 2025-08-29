using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Net.Http.Json;
using System.Diagnostics;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class OpenMeteoWeatherService : IWeatherDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IReverseGeocodingService _geocodingService;
        private readonly ILoggingService _loggingService;

        public string ProviderName => "Open-Meteo";
        public bool RequiresApiKey => false;

        public OpenMeteoWeatherService(IReverseGeocodingService geocodingService)
        {
            _httpClient = new HttpClient();
            _geocodingService = geocodingService;
            _loggingService = new CompositeLoggingService(); // File + Console logging
        }

        public OpenMeteoWeatherService(IReverseGeocodingService geocodingService, ILoggingService loggingService)
        {
            _httpClient = new HttpClient();
            _geocodingService = geocodingService;
            _loggingService = loggingService;
        }

        public async Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters)
        {
            var diagnostics = new ApiDiagnostics("OpenMeteoWeatherService", "GetHistoricalWeatherData");

            try
            {
                diagnostics.AddRequest();
                _loggingService.LogInformation($"Starting weather data request for location ({parameters.Location.Latitude:F4}, {parameters.Location.Longitude:F4}) - {parameters.YearsBack} years back");

                var locationData = await _geocodingService.GetLocationDataAsync(
                    parameters.Location.Latitude,
                    parameters.Location.Longitude);

                var endDate = parameters.EndDate ?? parameters.StartDate.AddDays(1);

                // Calculate date range for historical data
                var currentYear = DateTime.Now.Year;
                var startYear = currentYear - parameters.YearsBack;

                _loggingService.LogInformation($"Fetching weather data for years {startYear}-{currentYear}, date range {parameters.StartDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var allWeatherData = new List<WeatherData>();
                var failedYears = new List<int>();
                var successfulYears = 0;

                for (int year = startYear; year <= currentYear; year++)
                {
                    try
                    {
                        var yearlyData = await GetWeatherDataForYearAsync(
                            parameters.Location.Latitude,
                            parameters.Location.Longitude,
                            parameters.StartDate,
                            endDate,
                            year,
                            locationData);

                        if (yearlyData.Any())
                        {
                            allWeatherData.AddRange(yearlyData);
                            successfulYears++;
                            _loggingService.LogDebug($"Year {year}: Retrieved {yearlyData.Count} weather records");
                        }
                        else
                        {
                            failedYears.Add(year);
                            _loggingService.LogWarning($"Year {year}: No data returned");
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
                    ErrorMessage = $"Open-Meteo API error: {ex.Message}. See logs for details."
                };
            }
        }

        private async Task<List<WeatherData>> GetWeatherDataForYearAsync(
            double latitude, double longitude, DateTime startDate, DateTime endDate, int year, LocationData locationData)
        {
            var yearDiagnostics = new ApiDiagnostics("OpenMeteoWeatherService", $"GetWeatherDataYear{year}");

            try
            {
                yearDiagnostics.AddRequest();

                // Adjust dates for the specific year
                var yearStartDate = new DateTime(year, startDate.Month, startDate.Day);
                var yearEndDate = new DateTime(year, endDate.Month, endDate.Day);

                // If the target year is the current year and the date range is in the future, skip
                if (year == DateTime.Now.Year && yearStartDate > DateTime.Now)
                {
                    _loggingService.LogDebug($"Skipping year {year} - date range is in the future");
                    yearDiagnostics.Complete(true);
                    return new List<WeatherData>();
                }

                // Ensure coordinates use dots instead of commas (API requirement)
                var formattedLatitude = latitude.ToString("F6").Replace(',', '.');
                var formattedLongitude = longitude.ToString("F6").Replace(',', '.');

                var url = $"https://archive-api.open-meteo.com/v1/archive?" +
                         $"latitude={formattedLatitude}&longitude={formattedLongitude}" +
                         $"&start_date={yearStartDate:yyyy-MM-dd}&end_date={yearEndDate:yyyy-MM-dd}" +
                         $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
                         $"&timezone=auto";

                _loggingService.LogApiRequest($"OpenMeteo-Year{year}", url, new Dictionary<string, string>
                {
                    ["latitude"] = formattedLatitude,
                    ["longitude"] = formattedLongitude,
                    ["start_date"] = yearStartDate.ToString("yyyy-MM-dd"),
                    ["end_date"] = yearEndDate.ToString("yyyy-MM-dd"),
                    ["daily"] = "temperature_2m_max,temperature_2m_min,precipitation_sum",
                    ["timezone"] = "auto"
                });

                var requestStartTime = DateTime.Now;
                var response = await _httpClient.GetAsync(url);
                var requestDuration = DateTime.Now - requestStartTime;

                yearDiagnostics.SetStatusCode((int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    yearDiagnostics.AddError($"HTTP {(int)response.StatusCode}: {errorContent}");
                    _loggingService.LogApiResponse($"OpenMeteo-Year{year}", (int)response.StatusCode, errorContent, requestDuration);
                    _loggingService.LogError($"OpenMeteo API returned {(int)response.StatusCode} for year {year}: {errorContent}");
                    yearDiagnostics.Complete(false, (int)response.StatusCode);
                    return new List<WeatherData>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _loggingService.LogApiResponse($"OpenMeteo-Year{year}", (int)response.StatusCode, responseContent, requestDuration);

                var result = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>();

                if (result?.daily?.time == null || result.daily.temperature_2m_min == null || result.daily.temperature_2m_max == null || result.daily.precipitation_sum == null)
                {
                    yearDiagnostics.AddError("Invalid or incomplete response data");
                    _loggingService.LogWarning($"Year {year}: Invalid response structure from OpenMeteo API");
                    yearDiagnostics.Complete(false, (int)response.StatusCode);
                    return new List<WeatherData>();
                }

                var weatherData = new List<WeatherData>();
                for (int i = 0; i < result.daily.time.Length; i++)
                {
                    try
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
                    catch (Exception parseEx)
                    {
                        yearDiagnostics.AddError($"Data parsing error at index {i}: {parseEx.Message}");
                        _loggingService.LogWarning($"Failed to parse weather data at index {i} for year {year}: {parseEx.Message}");
                    }
                }

                yearDiagnostics.Complete(true, (int)response.StatusCode);
                _loggingService.LogDebug($"Year {year}: Successfully parsed {weatherData.Count} weather records");
                return weatherData;
            }
            catch (HttpRequestException ex)
            {
                yearDiagnostics.AddError($"Network error: {ex.Message}");
                yearDiagnostics.Complete(false);
                _loggingService.LogApiError($"OpenMeteo-Year{year}", $"https://archive-api.open-meteo.com/v1/archive", ex, yearDiagnostics.Duration ?? TimeSpan.Zero);
                _loggingService.LogError($"Network error fetching weather data for year {year}", ex);
                return new List<WeatherData>();
            }
            catch (Exception ex)
            {
                yearDiagnostics.AddError($"Unexpected error: {ex.Message}");
                yearDiagnostics.Complete(false);
                _loggingService.LogError($"Unexpected error processing year {year}", ex);
                return new List<WeatherData>();
            }
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