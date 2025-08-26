using HistoricWeatherData.Core.Models;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IGeolocationService
    {
        Task<LocationData?> GetLastKnownLocationAsync();
        Task<LocationData?> GetCurrentLocationAsync();
    }
}