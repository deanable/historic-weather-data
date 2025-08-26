using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.Services.Implementations;
using Syncfusion.Maui.Core.Hosting;

namespace HistoricWeatherData;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureSyncfusionCore();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register services
		builder.Services.AddSingleton<IReverseGeocodingService, ReverseGeocodingService>();
		builder.Services.AddSingleton<ISettingsService, SettingsService>();
		builder.Services.AddSingleton(Geolocation.Default);
		builder.Services.AddSingleton(Geocoding.Default);

		// Register weather providers
		builder.Services.AddTransient<IWeatherDataService, OpenMeteoWeatherService>();

		// Register ViewModels
		builder.Services.AddTransient<HistoricWeatherData.Core.ViewModels.MainViewModel>();
		builder.Services.AddTransient<HistoricWeatherData.Core.ViewModels.SettingsViewModel>();

		return builder.Build();
	}
}