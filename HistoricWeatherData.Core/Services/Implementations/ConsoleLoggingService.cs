using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.Models;
using System.Diagnostics;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class ConsoleLoggingService : ILoggingService
    {
        private readonly object _logLock = new();
        private readonly List<ApiErrorLog> _errorLogs = new();
        private readonly Dictionary<string, int> _errorCounts = new();

        public void LogInformation(string message)
        {
            lock (_logLock)
            {
                Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Debug.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }

        public void LogWarning(string message)
        {
            lock (_logLock)
            {
                Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Debug.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }

        public void LogError(string message, Exception? ex = null)
        {
            lock (_logLock)
            {
                var errorMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                if (ex != null)
                {
                    errorMessage += $"{Environment.NewLine}Exception: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}";
                }

                Console.Error.WriteLine(errorMessage);
                Debug.WriteLine(errorMessage);

                // Track error for diagnostics
                var logEntry = new ApiErrorLog
                {
                    Timestamp = DateTime.Now,
                    ErrorMessage = message,
                    ExceptionType = ex?.GetType().Name,
                    StackTrace = ex?.StackTrace
                };

                _errorLogs.Add(logEntry);

                // Update error count
                string key = ex?.GetType().Name ?? "Unknown";
                if (_errorCounts.ContainsKey(key))
                    _errorCounts[key]++;
                else
                    _errorCounts[key] = 1;
            }
        }

        public void LogDebug(string message)
        {
            lock (_logLock)
            {
                Console.WriteLine($"[DEBUG] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Debug.WriteLine($"[DEBUG] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }

        public void LogApiRequest(string serviceName, string url, Dictionary<string, string>? parameters = null)
        {
            lock (_logLock)
            {
                var message = $"[API REQUEST] {serviceName} - {url}";
                if (parameters != null && parameters.Count > 0)
                {
                    message += $" | Params: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}";
                }
                LogDebug(message);
            }
        }

        public void LogApiResponse(string serviceName, int statusCode, string responseContent, TimeSpan duration, Dictionary<string, string>? parameters = null)
        {
            lock (_logLock)
            {
                var truncatedResponse = responseContent.Length > 500
                    ? responseContent.Substring(0, 500) + "..."
                    : responseContent;

                var message = $"[API RESPONSE] {serviceName} - Status: {statusCode} | Duration: {duration.TotalMilliseconds:F0}ms | Response: {truncatedResponse}";
                LogDebug(message);
            }
        }

        public void LogApiError(string serviceName, string url, Exception ex, TimeSpan duration)
        {
            lock (_logLock)
            {
                var message = $"[API ERROR] {serviceName} - URL: {url} | Duration: {duration.TotalMilliseconds:F0}ms | Error: {ex.GetType().Name}: {ex.Message}";
                Console.Error.WriteLine(message);
                Debug.WriteLine(message);

                // Create detailed error log
                var logEntry = new ApiErrorLog
                {
                    Timestamp = DateTime.Now,
                    ServiceName = serviceName,
                    Url = url,
                    ErrorMessage = ex.Message,
                    ExceptionType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    ResponseTime = duration
                };

                _errorLogs.Add(logEntry);

                // Update error count
                string key = $"{serviceName}:{ex.GetType().Name}";
                if (_errorCounts.ContainsKey(key))
                    _errorCounts[key]++;
                else
                    _errorCounts[key] = 1;
            }
        }

        public IReadOnlyList<ApiErrorLog> GetErrorLogs()
        {
            lock (_logLock)
            {
                return _errorLogs.AsReadOnly();
            }
        }

        public Dictionary<string, int> GetErrorCounts()
        {
            lock (_logLock)
            {
                return new Dictionary<string, int>(_errorCounts);
            }
        }

        public void ClearErrorLogs()
        {
            lock (_logLock)
            {
                _errorLogs.Clear();
            }
        }

        public string GetLogDirectory() => string.Empty;

        public string GetLogFilePath() => string.Empty;

        public string GetErrorLogFilePath() => string.Empty;