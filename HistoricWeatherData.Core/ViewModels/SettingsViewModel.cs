using HistoricWeatherData.Core.Services.Interfaces;
using System.ComponentModel;
using System.Windows.Input;

namespace HistoricWeatherData.Core.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private string _syncfusionLicenseKey = string.Empty;
        private string _openWeatherMapKey = string.Empty;
        private string _visualCrossingKey = string.Empty;
        private string _weatherApiComKey = string.Empty;
        private string _nasaPowerKey = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isSuccess;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static async Task<SettingsViewModel> Create(ISettingsService settingsService)
        {
            var viewModel = new SettingsViewModel(settingsService);
            await viewModel.LoadSettingsAsync();
            return viewModel;
        }

        private SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            SaveSyncfusionKeyCommand = new RelayCommand(async () => await SaveSyncfusionKeyAsync());
            SaveApiKeysCommand = new RelayCommand(async () => await SaveApiKeysAsync());
            ClearAllCommand = new RelayCommand(async () => await ClearAllAsync());
            GoBackCommand = new RelayCommand(GoBack);
        }

        public string SyncfusionLicenseKey
        {
            get => _syncfusionLicenseKey;
            set
            {
                if (_syncfusionLicenseKey != value)
                {
                    _syncfusionLicenseKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OpenWeatherMapKey
        {
            get => _openWeatherMapKey;
            set
            {
                if (_openWeatherMapKey != value)
                {
                    _openWeatherMapKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VisualCrossingKey
        {
            get => _visualCrossingKey;
            set
            {
                if (_visualCrossingKey != value)
                {
                    _visualCrossingKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string WeatherApiComKey
        {
            get => _weatherApiComKey;
            set
            {
                if (_weatherApiComKey != value)
                {
                    _weatherApiComKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NasaPowerKey
        {
            get => _nasaPowerKey;
            set
            {
                if (_nasaPowerKey != value)
                {
                    _nasaPowerKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                if (_isSuccess != value)
                {
                    _isSuccess = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveSyncfusionKeyCommand { get; }
        public ICommand SaveApiKeysCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand GoBackCommand { get; }

        private async Task LoadSettingsAsync()
        {
            try
            {
                var syncfusionKey = await _settingsService.GetSyncfusionLicenseKeyAsync();
                if (!string.IsNullOrEmpty(syncfusionKey))
                {
                    SyncfusionLicenseKey = syncfusionKey;
                }

                var owmKey = await _settingsService.GetApiKeyAsync("OpenWeatherMap");
                if (!string.IsNullOrEmpty(owmKey))
                {
                    OpenWeatherMapKey = owmKey;
                }

                var vcKey = await _settingsService.GetApiKeyAsync("VisualCrossing");
                if (!string.IsNullOrEmpty(vcKey))
                {
                    VisualCrossingKey = vcKey;
                }

                var wacKey = await _settingsService.GetApiKeyAsync("WeatherApiCom");
                if (!string.IsNullOrEmpty(wacKey))
                {
                    WeatherApiComKey = wacKey;
                }

                var npKey = await _settingsService.GetApiKeyAsync("NasaPower");
                if (!string.IsNullOrEmpty(npKey))
                {
                    NasaPowerKey = npKey;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading settings: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task SaveSyncfusionKeyAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SyncfusionLicenseKey))
                {
                    StatusMessage = "Please enter a valid license key";
                    IsSuccess = false;
                    return;
                }

                await _settingsService.SaveSyncfusionLicenseKeyAsync(SyncfusionLicenseKey);
                StatusMessage = "Syncfusion license key saved successfully";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving license key: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task SaveApiKeysAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(OpenWeatherMapKey))
                {
                    await _settingsService.SaveApiKeyAsync("OpenWeatherMap", OpenWeatherMapKey);
                }

                if (!string.IsNullOrWhiteSpace(VisualCrossingKey))
                {
                    await _settingsService.SaveApiKeyAsync("VisualCrossing", VisualCrossingKey);
                }

                if (!string.IsNullOrWhiteSpace(WeatherApiComKey))
                {
                    await _settingsService.SaveApiKeyAsync("WeatherApiCom", WeatherApiComKey);
                }

                if (!string.IsNullOrWhiteSpace(NasaPowerKey))
                {
                    await _settingsService.SaveApiKeyAsync("NasaPower", NasaPowerKey);
                }

                StatusMessage = "API keys saved successfully";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving API keys: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task ClearAllAsync()
        {
            try
            {
                await _settingsService.ClearAllSettingsAsync();
                SyncfusionLicenseKey = string.Empty;
                OpenWeatherMapKey = string.Empty;
                VisualCrossingKey = string.Empty;
                WeatherApiComKey = string.Empty;
                NasaPowerKey = string.Empty;

                StatusMessage = "All settings cleared successfully";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing settings: {ex.Message}";
                IsSuccess = false;
            }
        }

        private void GoBack()
        {
            // Navigation will be handled by the UI layer
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}