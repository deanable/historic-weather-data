using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.ViewModels;

namespace HistoricWeatherData.WinForms
{
    public class MainForm : Form
    {
        private readonly MainViewModel _viewModel;
        private DataGridView weatherDataGrid = null!;
        private TextBox locationTextBox = null!;
        private ComboBox timeRangeComboBox = null!;
        private ComboBox weatherProviderComboBox = null!;
        private ComboBox yearsComboBox = null!;
        private CheckBox exportAveragesCheckBox = null!;
        private ComboBox exportFormatComboBox = null!;
        private DateTimePicker startDatePicker = null!;
        private DateTimePicker endDatePicker = null!;
        private Label endDateLabel = null!;
        private Button loadDataButton = null!;
        private Button exportDataButton = null!;
        private Button clearDataButton = null!;
        private Label statusLabel = null!;
        private ProgressBar loadingProgressBar = null!;
        private TableLayoutPanel mainPanel = null!;

        public MainForm()
        {
            // Initialize services and view model
            var geocodingService = new ReverseGeocodingService();
            var weatherService = new OpenMeteoWeatherService(geocodingService);
            var settingsService = new SettingsService();
            var dataExportService = new DataExportService();
            _viewModel = new MainViewModel(weatherService, geocodingService, settingsService, dataExportService);

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            InitializeComponents();
            SetupLayout();
        }

        private void InitializeComponents()
        {
            // Initialize controls
            weatherDataGrid = new DataGridView();
            locationTextBox = new TextBox();
            timeRangeComboBox = new ComboBox();
            weatherProviderComboBox = new ComboBox();
            yearsComboBox = new ComboBox();
            exportAveragesCheckBox = new CheckBox();
            exportFormatComboBox = new ComboBox();
            startDatePicker = new DateTimePicker();
            endDatePicker = new DateTimePicker();
            endDateLabel = new Label();
            loadDataButton = new Button();
            exportDataButton = new Button();
            clearDataButton = new Button();
            statusLabel = new Label();
            loadingProgressBar = new ProgressBar();
            mainPanel = new TableLayoutPanel();

            // Initialize end date label
            endDateLabel.Text = "End Date:";
            endDateLabel.AutoSize = true;

            // Configure main panel
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.RowCount = 3;
            mainPanel.ColumnCount = 1;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));  // Controls panel - increased height
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Status bar

            this.Size = new Size(1400, 900); // Increased width for more controls
        }

        private void SetupLayout()
        {
            Controls.Add(mainPanel);

            // Setup the control panel (row 0)
            var controlsPanel = CreateControlsPanel();
            mainPanel.Controls.Add(controlsPanel, 0, 0);

            // Setup data grid
            ConfigureDataGrid(weatherDataGrid);
            weatherDataGrid.DataSource = new BindingSource { DataSource = _viewModel.WeatherData };
            mainPanel.Controls.Add(weatherDataGrid, 0, 1);

            // Setup status panel (row 2)
            var statusPanel = CreateStatusPanel();
            mainPanel.Controls.Add(statusPanel, 0, 2);
        }

        private Panel CreateControlsPanel()
        {
            var panel = new Panel { Height = 140, Dock = DockStyle.Top };

            // Row 1: Location and Provider
            var locationLabel = new Label { Text = "Location:", Location = new Point(10, 10), AutoSize = true };
            locationTextBox.Text = _viewModel.LocationName;
            locationTextBox.Location = new Point(10, 30);
            locationTextBox.Width = 200;
            locationTextBox.TextChanged += (s, e) =>
            {
                _viewModel.LocationName = locationTextBox.Text;
                ValidateLoadButton();
            };

            var providerLabel = new Label { Text = "Weather Provider:", Location = new Point(300, 10), AutoSize = true };
            weatherProviderComboBox.DataSource = _viewModel.WeatherProviders;
            weatherProviderComboBox.SelectedItem = _viewModel.SelectedWeatherProvider;
            weatherProviderComboBox.Location = new Point(300, 30);
            weatherProviderComboBox.Width = 150;
            weatherProviderComboBox.SelectedIndexChanged += (s, e) =>
            {
                _viewModel.SelectedWeatherProvider = weatherProviderComboBox.SelectedItem?.ToString();
            };

            var yearsLabel = new Label { Text = "Years:", Location = new Point(500, 10), AutoSize = true };
            yearsComboBox.DataSource = _viewModel.Years;
            yearsComboBox.SelectedItem = _viewModel.SelectedYear;
            yearsComboBox.Location = new Point(500, 30);
            yearsComboBox.Width = 60;

            // Row 2: Time Range and Dates
            var timeRangeLabel = new Label { Text = "Time Range:", Location = new Point(10, 60), AutoSize = true };
            timeRangeComboBox.DataSource = _viewModel.TimeRanges;
            timeRangeComboBox.SelectedItem = _viewModel.SelectedTimeRange;
            timeRangeComboBox.Location = new Point(10, 80);
            timeRangeComboBox.Width = 120;
            timeRangeComboBox.SelectedIndexChanged += (s, e) =>
            {
                _viewModel.SelectedTimeRange = timeRangeComboBox.SelectedItem?.ToString() ?? "1 Week";
                UpdateDatePickerVisibility();
            };

            var startLabel = new Label { Text = "Start Date:", Location = new Point(150, 60), AutoSize = true };
            startDatePicker.Value = _viewModel.StartDate;
            startDatePicker.Location = new Point(150, 80);
            startDatePicker.Width = 120;
            startDatePicker.ValueChanged += (s, e) =>
            {
                _viewModel.StartDate = startDatePicker.Value;
                ValidateLoadButton();
            };

            endDateLabel.Location = new Point(300, 60);
            endDatePicker.Value = DateTime.Now;
            endDatePicker.Location = new Point(300, 80);
            endDatePicker.Width = 120;
            endDatePicker.ValueChanged += (s, e) =>
            {
                _viewModel.EndDate = endDatePicker.Value;
                ValidateLoadButton();
            };

            // Row 3: Export options
            var exportFormatLabel = new Label { Text = "Export Format:", Location = new Point(450, 60), AutoSize = true };
            exportFormatComboBox.DataSource = _viewModel.ExportFormats;
            exportFormatComboBox.SelectedItem = _viewModel.SelectedExportFormat;
            exportFormatComboBox.Location = new Point(450, 80);
            exportFormatComboBox.Width = 80;
            exportFormatComboBox.SelectedIndexChanged += (s, e) =>
            {
                _viewModel.SelectedExportFormat = exportFormatComboBox.SelectedItem?.ToString();
            };

            exportAveragesCheckBox.Text = "Include Averages";
            exportAveragesCheckBox.Checked = _viewModel.ExportAverages;
            exportAveragesCheckBox.Location = new Point(550, 80);
            exportAveragesCheckBox.AutoSize = true;
            exportAveragesCheckBox.CheckedChanged += (s, e) =>
            {
                _viewModel.ExportAverages = exportAveragesCheckBox.Checked;
            };

            // Action buttons
            loadDataButton.Text = "Load Weather Data";
            loadDataButton.Location = new Point(700, 30);
            loadDataButton.Width = 120;
            loadDataButton.Click += (s, e) => LoadWeatherData();

            exportDataButton.Text = "Export Data";
            exportDataButton.Location = new Point(700, 80);
            exportDataButton.Width = 100;
            exportDataButton.Click += (s, e) => ExportWeatherData();
            exportDataButton.Enabled = false; // Initially disabled

            clearDataButton.Text = "Clear Data";
            clearDataButton.Location = new Point(820, 30);
            clearDataButton.Width = 100;
            clearDataButton.Click += (s, e) => _viewModel.ClearDataCommand.Execute(null);

            // Initialize date picker visibility
            UpdateDatePickerVisibility();

            panel.Controls.AddRange(new Control[] {
                // Row 1
                locationLabel, locationTextBox,
                providerLabel, weatherProviderComboBox,
                yearsLabel, yearsComboBox,
                // Row 2
                timeRangeLabel, timeRangeComboBox,
                startLabel, startDatePicker,
                endDateLabel, endDatePicker,
                // Row 3
                exportFormatLabel, exportFormatComboBox, exportAveragesCheckBox,
                // Buttons
                loadDataButton, exportDataButton, clearDataButton
            });

            return panel;
        }

        private void ConfigureDataGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoGenerateColumns = true;
            grid.BackgroundColor = SystemColors.Window;
        }

        private Panel CreateStatusPanel()
        {
            var panel = new Panel { Height = 30, Dock = DockStyle.Bottom };

            statusLabel.Text = "Ready";
            statusLabel.Location = new Point(10, 8);
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Segoe UI", 9);

            loadingProgressBar.Location = new Point(1300, 8);
            loadingProgressBar.Width = 80;
            loadingProgressBar.Height = 15;
            loadingProgressBar.Visible = false;
            loadingProgressBar.Style = ProgressBarStyle.Marquee;

            panel.Controls.Add(statusLabel);
            panel.Controls.Add(loadingProgressBar);
            panel.BackColor = SystemColors.Control;

            return panel;
        }

        private void UpdateDatePickerVisibility()
        {
            bool isCustomRange = _viewModel.SelectedTimeRange == "Custom Range";

            endDateLabel.Visible = isCustomRange;
            endDatePicker.Visible = isCustomRange;

            // Update button validation
            ValidateLoadButton();
        }

        private void ValidateLoadButton()
        {
            bool isValid = true;
            string errorMessage = "";

            // Check if location is specified
            if (string.IsNullOrWhiteSpace(_viewModel.LocationName))
            {
                isValid = false;
                errorMessage = "Please enter a location.";
            }
            // Check if end date exists for custom range and is valid
            else if (_viewModel.SelectedTimeRange == "Custom Range")
            {
                if (endDatePicker.Value < startDatePicker.Value)
                {
                    isValid = false;
                    errorMessage = "End date must be after start date.";
                }
            }

            if (isValid)
            {
                loadDataButton.Enabled = !_viewModel.IsLoading;
                statusLabel.Text = "Ready";
            }
            else
            {
                loadDataButton.Enabled = false;
                statusLabel.Text = errorMessage;
            }

            // Update export button enable state
            exportDataButton.Enabled = _viewModel.WeatherData.Count > 0 && !_viewModel.IsLoading;
        }

        private void LoadWeatherData()
        {
            if (_viewModel.LoadWeatherDataCommand.CanExecute(null))
            {
                _viewModel.LoadWeatherDataCommand.Execute(null);
            }
        }

        private void ExportWeatherData()
        {
            if (_viewModel.ExportWeatherDataCommand.CanExecute(null))
            {
                _viewModel.ExportWeatherDataCommand.Execute(null);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ViewModel_PropertyChanged(sender, e)));
                return;
            }

            if (e.PropertyName == nameof(MainViewModel.StatusMessage))
            {
                statusLabel.Text = _viewModel.StatusMessage;
            }
            else if (e.PropertyName == nameof(MainViewModel.IsLoading))
            {
                loadingProgressBar.Visible = _viewModel.IsLoading;
                ValidateLoadButton();
            }
            else if (e.PropertyName == "WeatherData")
            {
                // Force refresh the data grid
                if (weatherDataGrid.DataSource is BindingSource bs)
                {
                    bs.ResetBindings(false);
                }
                // Update export button state
                exportDataButton.Enabled = _viewModel.WeatherData.Count > 0 && !_viewModel.IsLoading;
            }
            else if (e.PropertyName == nameof(MainViewModel.LocationName))
            {
                ValidateLoadButton();
            }
            else if (e.PropertyName == nameof(MainViewModel.SelectedTimeRange))
            {
                UpdateDatePickerVisibility();
            }
        }
    }
}