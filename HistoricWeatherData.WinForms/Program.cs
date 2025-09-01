using HistoricWeatherData.Core.Services;
using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using System;
using System.Windows.Forms;

namespace HistoricWeatherData.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZecXVTR2RYVkJ3WUVWYU8=");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        Application.Run(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MainForm>(ServiceProvider));
    }

    public static IServiceProvider? ServiceProvider { get; private set; }

    static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => {
                services.AddHttpClient();

                // Register Services
                services.AddSingleton<ILoggingService, CompositeLoggingService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IDataExportService, DataExportService>();
                services.AddSingleton<IReverseGeocodingService, ReverseGeocodingService>();
                services.AddSingleton<IWeatherServiceFactory, WeatherServiceFactory>();

                // Register ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<SettingsViewModel>(sp =>
                    SettingsViewModel.Create(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ISettingsService>(sp)).Result);

                // Register Forms
                services.AddTransient<MainForm>();
                services.AddTransient<SettingsForm>();

                // Register Forms
                services.AddTransient<MainForm>();
                services.AddTransient<SettingsForm>();
            });
}