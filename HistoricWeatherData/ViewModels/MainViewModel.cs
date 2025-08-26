using HistoricWeatherData.Models;
using HistoricWeatherData.Services.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace HistoricWeatherData.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherDataService _weatherService;
        private readonly IReverseGeocodingService _geocodingService;
        private readonly ISettingsService _settingsService;

        private DateTime _startDate = DateTime.Now.AddDays(-7);
        private DateTime? _endDate;
        private int _yearsBack = 1;
        private string _selectedTimeRange = "1 Week";
        private double _latitude = 40.7128; // Default to New York
        private double _longitude = -74.0060;
        private string _locationName = "New York, NY";
        private bool _isLoading;
        private string _statusMessage = "Ready";

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IWeatherDataService weatherService, IReverseGeocodingService geocodingService, ISettingsService settingsService)
        {
            _weatherService = weatherService;
            _geocodingService = geocodingService;
            _settingsService = settingsService;

            WeatherData = new ObservableCollection<WeatherData>();
            TimeRanges = new ObservableCollection<string>
            {
                "1 Day", "1 Week", "14 Days", "30 Days",
                "3 Months", "6 Months", "12 Months", "Custom Range"
            };

            LoadWeatherDataCommand = new Command(async () => await LoadWeatherDataAsync());
            ClearDataCommand = new Command(ClearData);
            NavigateToSettingsCommand = new Command(NavigateToSettings);
            GetCurrentLocationCommand = new Command(async () => await GetCurrentLocationAsync());
        }

        public ObservableCollection<WeatherData> WeatherData { get; }
        public ObservableCollection<string> TimeRanges { get; }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                    UpdateEndDate();
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public int YearsBack
        {
            get => _yearsBack;
            set
            {
                if (_yearsBack != value)
                {
                    _yearsBack = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                if (_selectedTimeRange != value)
                {
                    _selectedTimeRange = value;
                    OnPropertyChanged();
                    UpdateDateRange();
                }
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LocationName
        {
            get => _locationName;
            set
            {
                if (_locationName != value)
                {
                    _locationName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
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

        public ICommand LoadWeatherDataCommand { get; }
        public ICommand ClearDataCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand GetCurrentLocationCommand { get; }

        private void UpdateDateRange()
        {
            var now = DateTime.Now;
            switch (SelectedTimeRange)
            {
                case "1 Day":
                    StartDate = now.AddDays(-1);
                    EndDate = null;
                    break;
                case "1 Week":
                    StartDate = now.AddDays(-7);
                    EndDate = null;
                    break;
                case "14 Days":
                    StartDate = now.AddDays(-14);
                    EndDate = null;
                    break;
                case "30 Days":
                    StartDate = now.AddDays(-30);
                    EndDate = null;
                    break;
                case "3 Months":
                    StartDate = now.AddMonths(-3);
                    EndDate = null;
                    break;
                case "6 Months":
                    StartDate = now.AddMonths(-6);
                    EndDate = null;
                    break;
                case "12 Months":
                    StartDate = now.AddMonths(-12);
                    EndDate = null;
                    break;
                case "Custom Range":
                    // Keep current values
                    break;
            }
        }

        private void UpdateEndDate()
        {
            if (SelectedTimeRange != "Custom Range")
            {
                EndDate = null;
            }
        }

        private async Task GetCurrentLocationAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Getting current location...";

                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
                }

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;

                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        LocationName = $"{placemark.Locality}, {placemark.AdminArea}";
                    }
                    else
                    {
                        LocationName = "Unknown";
                    }

                    StatusMessage = "Current location found";
                }
                else
                {
                    StatusMessage = "Unable to get current location";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWeatherDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading weather data...";

                var locationData = new LocationData
                {
                    Latitude = Latitude,
                    Longitude = Longitude,
                    CityName = LocationName
                };

                var parameters = new WeatherQueryParameters
                {
                    Location = locationData,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    YearsBack = YearsBack
                };

                var response = await _weatherService.GetHistoricalWeatherDataAsync(parameters);

                if (response.IsSuccess)
                {
                    WeatherData.Clear();
                    foreach (var data in response.Data)
                    {
                        WeatherData.Add(data);
                    }
                    StatusMessage = $"Loaded {response.Data.Count} weather records";
                }
                else
                {
                    StatusMessage = $"Error: {response.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearData()
        {
            WeatherData.Clear();
            StatusMessage = "Data cleared";
        }

        private async void NavigateToSettings()
        {
            var settingsViewModel = await SettingsViewModel.Create(_settingsService);
            var settingsPage = new Views.SettingsPage(settingsViewModel);
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(settingsPage);
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}