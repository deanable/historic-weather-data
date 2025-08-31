using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace HistoricWeatherData.Core.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherDataService _weatherService;
        private readonly IReverseGeocodingService _geocodingService;
        private readonly ISettingsService _settingsService;
        private readonly IDataExportService _dataExportService;

        private DateTime _startDate = DateTime.Now.AddDays(-7);
        private DateTime? _endDate;
        private int _selectedYear = 1;
        private string _selectedTimeRange = "1 Week";
        private double _latitude = 40.7128; // Default to New York
        private double _longitude = -74.0060;
        private string _locationName = "New York, NY";
        private bool _isLoading;
        private string _statusMessage = "Ready";
        private string? _selectedWeatherProvider;
        private bool _exportAverages;
        private string? _selectedExportFormat;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IWeatherDataService weatherService, IReverseGeocodingService geocodingService, ISettingsService settingsService, IDataExportService dataExportService)
        {
            _weatherService = weatherService;
            _geocodingService = geocodingService;
            _settingsService = settingsService;
            _dataExportService = dataExportService;

            WeatherData = new ObservableCollection<WeatherData>();
            TimeRanges = new ObservableCollection<string>
            {
                "1 Day", "1 Week", "14 Days", "30 Days",
                "3 Months", "6 Months", "12 Months", "Custom Range"
            };
            WeatherProviders = new ObservableCollection<string>
            {
                "OpenMeteo",
                "Dummy Provider"
            };
            SelectedWeatherProvider = WeatherProviders.First();
            Years = new ObservableCollection<int>(Enumerable.Range(1, 10));
            SelectedYear = Years.First();
            ExportFormats = new ObservableCollection<string> { "CSV", "Excel" };
            SelectedExportFormat = ExportFormats.First();

            LoadWeatherDataCommand = new RelayCommand(async () => await LoadWeatherDataAsync());
            ClearDataCommand = new RelayCommand(ClearData);
            NavigateToSettingsCommand = new RelayCommand(async () => await NavigateToSettings());
            ExportWeatherDataCommand = new RelayCommand(async () => await ExportWeatherDataAsync());
        }

        public ObservableCollection<WeatherData> WeatherData { get; }
        public ObservableCollection<string> WeatherProviders { get; }
        public ObservableCollection<int> Years { get; }
        public ObservableCollection<string> ExportFormats { get; }

        private void NotifyWeatherDataChanged()
        {
            OnPropertyChanged("WeatherData");
        }
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

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? SelectedWeatherProvider
        {
            get => _selectedWeatherProvider;
            set
            {
                if (_selectedWeatherProvider != value)
                {
                    _selectedWeatherProvider = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ExportAverages
        {
            get => _exportAverages;
            set
            {
                if (_exportAverages != value)
                {
                    _exportAverages = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? SelectedExportFormat
        {
            get => _selectedExportFormat;
            set
            {
                if (_selectedExportFormat != value)
                {
                    _selectedExportFormat = value;
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
        public ICommand ExportWeatherDataCommand { get; }

        private void UpdateDateRange()
        {
            var now = DateTime.Now;
            switch (SelectedTimeRange)
            {
                case "1 Day":
                    StartDate = now.AddDays(-1);
                    EndDate = now;
                    break;
                case "1 Week":
                    StartDate = now.AddDays(-7);
                    EndDate = now;
                    break;
                case "14 Days":
                    StartDate = now.AddDays(-14);
                    EndDate = now;
                    break;
                case "30 Days":
                    StartDate = now.AddDays(-30);
                    EndDate = now;
                    break;
                case "3 Months":
                    StartDate = now.AddMonths(-3);
                    EndDate = now;
                    break;
                case "6 Months":
                    StartDate = now.AddMonths(-6);
                    EndDate = now;
                    break;
                case "12 Months":
                    StartDate = now.AddMonths(-12);
                    EndDate = now;
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
                    YearsBack = SelectedYear,
                    ProviderName = SelectedWeatherProvider ?? string.Empty
                };

                var response = await _weatherService.GetHistoricalWeatherDataAsync(parameters);

                if (response.IsSuccess)
                {
                    WeatherData.Clear();
                    foreach (var data in response.Data)
                    {
                        WeatherData.Add(data);
                    }
                    NotifyWeatherDataChanged();
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

        private async Task ExportWeatherDataAsync()
        {
            if (WeatherData.Count == 0)
            {
                StatusMessage = "No weather data to export.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Exporting weather data...";

                var location = LocationName;
                var dataToExport = new List<WeatherData>(WeatherData);

                if (SelectedExportFormat == "CSV")
                {
                    await _dataExportService.ExportDataAsCsvAsync(location, dataToExport, ExportAverages);
                }
                else
                {
                    await _dataExportService.ExportDataAsExcelAsync(location, dataToExport, ExportAverages);
                }

                StatusMessage = "Weather data export initiated.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearData()
        {
            WeatherData.Clear();
            NotifyWeatherDataChanged();
            StatusMessage = "Data cleared";
        }

        private async Task NavigateToSettings()
        {
            // Navigation will be handled by the UI layer
            await Task.CompletedTask;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}