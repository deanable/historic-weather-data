
namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<string?> GetApiKeyAsync(string providerName);
        Task SaveApiKeyAsync(string providerName, string apiKey);
        Task<string?> GetSyncfusionLicenseKeyAsync();
        Task SaveSyncfusionLicenseKeyAsync(string licenseKey);
        Task ClearAllSettingsAsync();
    }
}
