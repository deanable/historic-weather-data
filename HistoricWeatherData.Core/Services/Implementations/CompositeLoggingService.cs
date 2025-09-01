using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Implementations;
using HistoricWeatherData.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

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

    public void LogApiError(string serviceName, string url, Exception ex, TimeSpan duration)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiError(serviceName, url, ex, duration);
        }
    }

    public void LogApiRequest(string serviceName, string url, Dictionary<string, string>? parameters = null)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiRequest(serviceName, url, parameters);
        }
    }

    public void LogApiResponse(string serviceName, int statusCode, string content, TimeSpan duration, Dictionary<string, string>? parameters = null)
    {
        foreach (var service in _loggingServices)
        {
            service.LogApiResponse(serviceName, statusCode, content, duration, parameters);
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

    public void LogDebug(string message)
    {
        foreach (var service in _loggingServices)
        {
            service.LogDebug(message);
        }
    }

    public IReadOnlyList<ApiErrorLog> GetErrorLogs()
    {
        // Return logs from the first service that provides them (likely FileLoggingService)
        return _loggingServices.Select(s => s.GetErrorLogs()).FirstOrDefault(l => l != null && l.Count > 0) ?? new List<ApiErrorLog>().AsReadOnly();
    }

    public Dictionary<string, int> GetErrorCounts()
    {
        // Return counts from the first service that provides them
        return _loggingServices.Select(s => s.GetErrorCounts()).FirstOrDefault(c => c != null && c.Count > 0) ?? new Dictionary<string, int>();
    }

    public void ClearErrorLogs()
    {
        foreach (var service in _loggingServices)
        {
            service.ClearErrorLogs();
        }
    }

    public string GetLogDirectory()
    {
        return _loggingServices.Select(s => s.GetLogDirectory()).FirstOrDefault(d => !string.IsNullOrEmpty(d)) ?? string.Empty;
    }

    public string GetLogFilePath()
    {
        return _loggingServices.Select(s => s.GetLogFilePath()).FirstOrDefault(f => !string.IsNullOrEmpty(f)) ?? string.Empty;
    }

    public string GetErrorLogFilePath()
    {
        return _loggingServices.Select(s => s.GetErrorLogFilePath()).FirstOrDefault(f => !string.IsNullOrEmpty(f)) ?? string.Empty;
    }
}
