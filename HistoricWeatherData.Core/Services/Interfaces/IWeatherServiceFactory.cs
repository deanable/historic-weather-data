namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IWeatherServiceFactory
    {
        IWeatherDataService GetService(string providerName);
    }
}