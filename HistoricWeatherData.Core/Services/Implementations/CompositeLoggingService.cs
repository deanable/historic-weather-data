using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;

public class CompositeLoggingService : ILoggingService
{
    private readonly ILoggingService[] _loggingServices;

    public CompositeLoggingService()
    {
        _loggingServices = new ILoggingService[]
        {
            new ConsoleLoggingService(),
            new FileLoggingService() 
        };
    }

    public void LogApiError(string apiName, string requestUrl, Exception ex, TimeSpan duration)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiError(apiName, requestUrl, ex, duration);
        }
    }

    public void LogApiRequest(string apiName, string requestUrl, Dictionary<string, string> parameters)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiRequest(apiName, requestUrl, parameters);
        }
    }

    public void LogApiResponse(string apiName, int statusCode, string responseContent, TimeSpan duration)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiResponse(apiName, statusCode, responseContent, duration);
        }
    }

    public void LogError(string message, Exception? ex = null)
    {
        foreach (var service in _loggingServices)
        {
            service.LogError(message, ex);
        }
    }

    public void LogInformation(string message)
    {
        foreach (var service in _loggingServices)
        {
            service.LogInformation(message);
        }
    }

    public void LogWarning(string message)
    {
        foreach (var service in _loggingServices)
        {
            service.LogWarning(message);
        }
    }
}