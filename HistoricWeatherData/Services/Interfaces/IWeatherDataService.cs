using HistoricWeatherData.Models;

namespace HistoricWeatherData.Services.Interfaces
{
    public interface IWeatherDataService
    {
        Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters);
        string ProviderName { get; }
        bool RequiresApiKey { get; }
    }

    public interface IReverseGeocodingService
    {
        Task<LocationData> GetLocationDataAsync(double latitude, double longitude);
    }

    public interface ISettingsService
    {
        Task<string?> GetApiKeyAsync(string providerName);
        Task SaveApiKeyAsync(string providerName, string apiKey);
        Task<string?> GetSyncfusionLicenseKeyAsync();
        Task SaveSyncfusionLicenseKeyAsync(string licenseKey);
        Task ClearAllSettingsAsync();
    }
}