using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.Services.Implementations;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace HistoricWeatherData;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Add Syncfusion license
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXddcHZVRWVfUkx3W0tWYEk=");

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
		builder.Services.AddSingleton<ILoggingService, CompositeLoggingService>();
		builder.Services.AddSingleton<IReverseGeocodingService, ReverseGeocodingService>();
		builder.Services.AddSingleton<ISettingsService, SettingsService>();
		builder.Services.AddSingleton<IDataExportService, DataExportService>();
		builder.Services.AddSingleton(Geolocation.Default);
		builder.Services.AddSingleton(Geocoding.Default);
		builder.Services.AddSingleton<IWeatherServiceFactory, WeatherServiceFactory>();

		// Register weather providers
		builder.Services.AddTransient<OpenMeteoWeatherService>();
		builder.Services.AddTransient<VisualCrossingWeatherService>();

		// Register ViewModels
		builder.Services.AddTransient<HistoricWeatherData.Core.ViewModels.MainViewModel>();
		builder.Services.AddTransient<HistoricWeatherData.Core.ViewModels.SettingsViewModel>();

		return builder.Build();
	}
}