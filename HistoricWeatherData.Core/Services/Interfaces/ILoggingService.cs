using HistoricWeatherData.Core.Models;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogDebug(string message);
        void LogApiRequest(string serviceName, string url, Dictionary<string, string> parameters = null);
        void LogApiResponse(string serviceName, int statusCode, string responseContent, TimeSpan duration);
        void LogApiError(string serviceName, string url, Exception ex, TimeSpan duration);
    }
}