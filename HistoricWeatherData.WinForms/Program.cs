using HistoricWeatherData.Core.Services;
using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        Application.Run(ServiceProvider.GetRequiredService<MainForm>());
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
                    SettingsViewModel.Create(sp.GetRequiredService<ISettingsService>()).Result);

                // Register Forms
                services.AddTransient<MainForm>();
                services.AddTransient<SettingsForm>();
            });
}