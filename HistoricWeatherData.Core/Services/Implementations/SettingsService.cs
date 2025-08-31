using HistoricWeatherData.Core.Services.Interfaces;
using System.Text.Json;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class SettingsService : ISettingsService
    {
        private const string SyncfusionKeyName = "SyncfusionLicenseKey";
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;
        private readonly object _fileLock = new();

        public SettingsService()
        {
            _settingsFilePath = GetSettingsFilePath();
            EnsureDirectoryExists(Path.GetDirectoryName(_settingsFilePath)!);
        }

        private static string GetSettingsFilePath()
        {
#if WINDOWS
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HistoricWeatherData");
            return Path.Combine(appDataPath, SettingsFileName);
#else
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HistoricWeatherData");
            return Path.Combine(appDataPath, SettingsFileName);
#endif
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public async Task<string?> GetApiKeyAsync(string providerName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_fileLock)
                    {
                        if (!File.Exists(_settingsFilePath))
                        {
                            return null;
                        }

                        var json = File.ReadAllText(_settingsFilePath);
                        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

                        return settings.TryGetValue(providerName, out var value) ? value : null;
                    }
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
                    lock (_fileLock)
                    {
                        Dictionary<string, string> settings;

                        if (File.Exists(_settingsFilePath))
                        {
                            var json = File.ReadAllText(_settingsFilePath);
                            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                        }
                        else
                        {
                            settings = new Dictionary<string, string>();
                        }

                        settings[providerName] = apiKey;

                        var updatedJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(_settingsFilePath, updatedJson);
                    }
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
                    lock (_fileLock)
                    {
                        if (File.Exists(_settingsFilePath))
                        {
                            File.Delete(_settingsFilePath);
                        }
                    }
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