using HistoricWeatherData.Core.Models;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IWeatherDataService
    {
        Task<WeatherResponse> GetHistoricalWeatherDataAsync(WeatherQueryParameters parameters);
        string ProviderName { get; }
        bool RequiresApiKey { get; }
    }

    
}