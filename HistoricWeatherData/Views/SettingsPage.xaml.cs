using HistoricWeatherData.Services.Interfaces;

namespace HistoricWeatherData.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage(ISettingsService settingsService)
        {
            // The ViewModel is set in XAML with dependency injection
        }
    }
}