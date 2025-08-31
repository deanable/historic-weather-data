using HistoricWeatherData.Core.Services.Interfaces;

namespace HistoricWeatherData.Core.Services
{
    public interface IWeatherServiceFactory
    {
        IWeatherDataService GetService(string providerName);
    }
}
