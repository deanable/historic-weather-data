using HistoricWeatherData.Services.Interfaces;
using HistoricWeatherData.ViewModels;

namespace HistoricWeatherData;

public partial class MainPage : ContentPage
{
	public MainPage(IWeatherDataService weatherService, IReverseGeocodingService geocodingService, ISettingsService settingsService)
	{
		InitializeComponent();

		// Set up ViewModel with dependency injection
		BindingContext = new MainViewModel(weatherService, geocodingService, settingsService);
	}
}

