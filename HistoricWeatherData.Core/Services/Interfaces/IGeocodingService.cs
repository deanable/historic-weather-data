using HistoricWeatherData.Core.Models;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IGeocodingService
    {
        Task<IEnumerable<LocationData>> GetPlacemarksAsync(double latitude, double longitude);
    }
}