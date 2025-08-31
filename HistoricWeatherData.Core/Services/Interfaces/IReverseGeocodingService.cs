using HistoricWeatherData.Core.Models;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IReverseGeocodingService
    {
        Task<LocationData> GetLocationDataAsync(double latitude, double longitude);
    }
}
