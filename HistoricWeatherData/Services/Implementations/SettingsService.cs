using HistoricWeatherData.Services.Interfaces;
using Microsoft.Win32;

namespace HistoricWeatherData.Services.Implementations
{
    public class SettingsService : ISettingsService
    {
        private const string RegistryKey = @"SOFTWARE\HistoricWeatherData";
        private const string SyncfusionKeyName = "SyncfusionLicenseKey";

        public async Task<string?> GetApiKeyAsync(string providerName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(RegistryKey);
                    return key?.GetValue(providerName)?.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading API key for {providerName}: {ex.Message}");
                    return null;
                }
            });
        }

        public async Task SaveApiKeyAsync(string providerName, string apiKey)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.CreateSubKey(RegistryKey);
                    key?.SetValue(providerName, apiKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving API key for {providerName}: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<string?> GetSyncfusionLicenseKeyAsync()
        {
            return await GetApiKeyAsync(SyncfusionKeyName);
        }

        public async Task SaveSyncfusionLicenseKeyAsync(string licenseKey)
        {
            await SaveApiKeyAsync(SyncfusionKeyName, licenseKey);
        }

        public async Task ClearAllSettingsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RegistryKey, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing settings: {ex.Message}");
                    throw;
                }
            });
        }
    }
}