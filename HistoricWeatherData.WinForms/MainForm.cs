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
        private DataGridView weatherDataGrid;
        private TextBox locationTextBox;
        private ComboBox timeRangeComboBox;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private Label endDateLabel;
        private Button loadDataButton;
        private Button clearDataButton;
        private Label statusLabel;
        private ProgressBar loadingProgressBar;
        private TableLayoutPanel mainPanel;

        public MainForm()
        {
            // Initialize services and view model
            var geocodingService = new ReverseGeocodingService();
            var weatherService = new OpenMeteoWeatherService(geocodingService);
            var settingsService = new SettingsService();
            _viewModel = new MainViewModel(weatherService, geocodingService, settingsService);

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
            startDatePicker = new DateTimePicker();
            endDatePicker = new DateTimePicker();
            endDateLabel = new Label();
            loadDataButton = new Button();
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
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));  // Controls panel
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Status bar

            this.Size = new Size(1200, 800);
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
            var panel = new Panel { Height = 100, Dock = DockStyle.Top };

            // Location input
            var locationLabel = new Label { Text = "Location:", Location = new Point(10, 10), AutoSize = true };
            locationTextBox.Text = _viewModel.LocationName;
            locationTextBox.Location = new Point(10, 30);
            locationTextBox.Width = 200;
            locationTextBox.TextChanged += (s, e) =>
            {
                _viewModel.LocationName = locationTextBox.Text;
                ValidateLoadButton();
            };

            // Time range selector
            var timeRangeLabel = new Label { Text = "Time Range:", Location = new Point(250, 10), AutoSize = true };
            timeRangeComboBox.DataSource = _viewModel.TimeRanges;
            timeRangeComboBox.SelectedItem = _viewModel.SelectedTimeRange;
            timeRangeComboBox.Location = new Point(250, 30);
            timeRangeComboBox.Width = 120;
            timeRangeComboBox.SelectedIndexChanged += (s, e) =>
            {
                _viewModel.SelectedTimeRange = timeRangeComboBox.SelectedItem?.ToString() ?? "1 Week";
                UpdateDatePickerVisibility();
            };

            // Date range inputs
            var startLabel = new Label { Text = "Start Date:", Location = new Point(400, 10), AutoSize = true };
            startDatePicker.Value = _viewModel.StartDate;
            startDatePicker.Location = new Point(400, 30);
            startDatePicker.Width = 120;
            startDatePicker.ValueChanged += (s, e) =>
            {
                _viewModel.StartDate = startDatePicker.Value;
                ValidateLoadButton();
            };

            // End date controls (initially hidden)
            endDateLabel.Location = new Point(550, 10);
            endDatePicker.Value = DateTime.Now;
            endDatePicker.Location = new Point(550, 30);
            endDatePicker.Width = 120;
            endDatePicker.ValueChanged += (s, e) =>
            {
                _viewModel.EndDate = endDatePicker.Value;
                ValidateLoadButton();
            };

            // Action buttons
            loadDataButton.Text = "Load Weather Data";
            loadDataButton.Location = new Point(700, 30);
            loadDataButton.Width = 120;
            loadDataButton.Click += (s, e) => LoadWeatherData();

            clearDataButton.Text = "Clear Data";
            clearDataButton.Location = new Point(850, 30);
            clearDataButton.Width = 120;
            clearDataButton.Click += (s, e) => _viewModel.ClearDataCommand.Execute(null);

            // Initialize date picker visibility
            UpdateDatePickerVisibility();

            panel.Controls.AddRange(new Control[] {
                locationLabel, locationTextBox,
                timeRangeLabel, timeRangeComboBox,
                startLabel, startDatePicker,
                endDateLabel, endDatePicker,
                loadDataButton, clearDataButton
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

            loadingProgressBar.Location = new Point(1100, 8);
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
        }

        private void LoadWeatherData()
        {
            if (_viewModel.LoadWeatherDataCommand.CanExecute(null))
            {
                _viewModel.LoadWeatherDataCommand.Execute(null);
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