using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public abstract class BaseWeatherDataService : IWeatherDataService
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILoggingService LoggingService;
        protected readonly ISettingsService SettingsService;

        public abstract string ProviderName { get; }
        public virtual bool RequiresApiKey => false;

        protected BaseWeatherDataService(ILoggingService loggingService, ISettingsService settingsService, HttpClient httpClient)
        {
            LoggingService = loggingService;
            SettingsService = settingsService;
            HttpClient = httpClient;
        }

        public async Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters)
        {
            string? apiKey = null;
            if (RequiresApiKey)
            {
                apiKey = await SettingsService.GetApiKeyAsync(ProviderName);
                if (string.IsNullOrEmpty(apiKey))
                {
                    return new WeatherResponse { IsSuccess = false, ErrorMessage = $"API key for {ProviderName} is not set." };
                }
            }

            var diagnostics = new ApiDiagnostics(GetType().Name, "GetHistoricalWeatherDataAsync");

            try
            {
                diagnostics.AddRequest();
                LoggingService.LogInformation($"Starting weather data request for location ({parameters.Location.Latitude:F4}, {parameters.Location.Longitude:F4}) using {ProviderName}");

                var allWeatherData = new List<WeatherData>();
                var failedDates = new List<DateTime>();
                var successfulDates = 0;

                var startDate = parameters.StartDate;
                var endDate = parameters.EndDate ?? DateTime.Now;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    try
                    {
                        var dailyData = await FetchDayDataAsync(parameters, date, apiKey, diagnostics);
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
                    catch (ApiAuthenticationException authEx)
                    {
                        // If we get an auth error, stop processing immediately.
                        diagnostics.AddError($"Authentication error: {authEx.Message}");
                        LoggingService.LogError($"Authentication failed for {ProviderName}", authEx);
                        return new WeatherResponse { IsSuccess = false, ErrorMessage = authEx.Message };
                    }
                    catch (Exception dateEx)
                    {
                        failedDates.Add(date);
                        diagnostics.AddError($"Date {date:yyyy-MM-dd}: {dateEx.Message}");
                        LoggingService.LogError($"Failed to retrieve data from {ProviderName} for date {date:yyyy-MM-dd}", dateEx);
                    }
                }

                var response = new WeatherResponse
                {
                    Data = allWeatherData,
                    IsSuccess = true
                };

                diagnostics.Complete(true);

                var summary = $"Completed {ProviderName} request: {allWeatherData.Count} total records from {successfulDates} dates";
                if (failedDates.Any())
                {
                    summary += $", {failedDates.Count} failed dates: {string.Join(", ", failedDates.Select(d => d.ToString("yyyy-MM-dd")))}";
                }

                LoggingService.LogInformation(summary);
                return response;
            }
            catch (Exception ex)
            {
                diagnostics.Complete(false);
                LoggingService.LogError($"Critical error in {ProviderName} data retrieval", ex);
                LoggingService.LogInformation(diagnostics.GetSummary());

                return new WeatherResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"{ProviderName} API error: {ex.Message}. See logs for details."
                };
            }
        }

        protected abstract Task<WeatherData?> FetchDayDataAsync(WeatherQueryParameters parameters, DateTime date, string? apiKey, ApiDiagnostics diagnostics);
    }
}
