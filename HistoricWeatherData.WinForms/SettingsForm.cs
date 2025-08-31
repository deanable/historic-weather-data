using System;
using System.Windows.Forms;
using HistoricWeatherData.Core.ViewModels;
using HistoricWeatherData.Core.Services.Interfaces;

namespace HistoricWeatherData.WinForms
{
    public class SettingsForm : Form
    {
        private readonly SettingsViewModel _viewModel;

        private TextBox openWeatherMapKeyTextBox = null!;
        private TextBox weatherApiKeyTextBox = null!;
        private Button saveButton = null!;
        private Label statusLabel = null!;

        public SettingsForm(ISettingsService settingsService)
        {
            _viewModel = SettingsViewModel.Create(settingsService).Result;
            InitializeComponents();
            BindControls();
        }

        private void InitializeComponents()
        {
            this.Text = "Settings";
            this.Size = new System.Drawing.Size(400, 300);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // OpenWeatherMap
            mainLayout.Controls.Add(new Label { Text = "OpenWeatherMap API Key:", AutoSize = true }, 0, 0);
            openWeatherMapKeyTextBox = new TextBox { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(openWeatherMapKeyTextBox, 1, 0);

            // WeatherAPI
            mainLayout.Controls.Add(new Label { Text = "WeatherAPI API Key:", AutoSize = true }, 0, 1);
            weatherApiKeyTextBox = new TextBox { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(weatherApiKeyTextBox, 1, 1);

            // Save Button
            saveButton = new Button { Text = "Save", Dock = DockStyle.Fill };
            saveButton.Click += async (s, e) => await _viewModel.SaveApiKeysCommand.ExecuteAsync(null);
            mainLayout.Controls.Add(saveButton, 1, 2);

            // Status Label
            statusLabel = new Label { Dock = DockStyle.Fill, AutoSize = true };
            mainLayout.Controls.Add(statusLabel, 0, 3);
            mainLayout.SetColumnSpan(statusLabel, 2);

            this.Controls.Add(mainLayout);
        }

        private void BindControls()
        {
            openWeatherMapKeyTextBox.DataBindings.Add("Text", _viewModel, nameof(SettingsViewModel.OpenWeatherMapKey), false, DataSourceUpdateMode.OnPropertyChanged);
            weatherApiKeyTextBox.DataBindings.Add("Text", _viewModel, nameof(SettingsViewModel.WeatherAPIKey), false, DataSourceUpdateMode.OnPropertyChanged);
            statusLabel.DataBindings.Add("Text", _viewModel, nameof(SettingsViewModel.StatusMessage), false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}
