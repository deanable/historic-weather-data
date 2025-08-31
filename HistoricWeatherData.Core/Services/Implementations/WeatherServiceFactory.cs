using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;
using System;

namespace HistoricWeatherData.Core.Services
{
    public class WeatherServiceFactory : IWeatherServiceFactory
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly IReverseGeocodingService _reverseGeocodingService;

        public WeatherServiceFactory(ILoggingService loggingService, ISettingsService settingsService, IReverseGeocodingService reverseGeocodingService)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
            _reverseGeocodingService = reverseGeocodingService;
        }

        public IWeatherDataService GetService(string providerName)
        {
            switch (providerName)
            {
                case "OpenMeteo":
                    return new OpenMeteoWeatherService(_reverseGeocodingService, _loggingService);
                case "Visual Crossing":
                    return new VisualCrossingWeatherService(_loggingService, _settingsService);
                case "OpenWeatherMap":
                    return new OpenWeatherMapWeatherService(_loggingService, _settingsService);
                case "WeatherAPI":
                    return new WeatherAPIWeatherService(_loggingService, _settingsService);
                default:
                    throw new NotSupportedException($"The weather provider '{providerName}' is not supported.");
            }
        }
    }
}
