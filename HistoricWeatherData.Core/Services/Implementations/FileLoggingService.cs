using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.Models;
using System.Diagnostics;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class FileLoggingService : ILoggingService
    {
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly string _errorLogFilePath;
        private readonly object _logLock = new();
        private readonly List<ApiErrorLog> _errorLogs = new();
        private readonly Dictionary<string, int> _errorCounts = new();

        public FileLoggingService(string? logFilePath = null)
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            // Create logs directory if it doesn't exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (string.IsNullOrEmpty(logFilePath))
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
                _logFilePath = Path.Combine(_logDirectory, $"HistoricWeatherData_{timestamp}.log");
                _errorLogFilePath = Path.Combine(_logDirectory, "errors.log");
            }
            else
            {
                _logFilePath = logFilePath;
                _errorLogFilePath = logFilePath.Replace(".log", "_errors.log");
            }

            // Write header to log file
            WriteToFile($"=== Historic Weather Data Log Started: {DateTime.Now} ===", _logFilePath);
            WriteToFile($"=== Historic Weather Data Error Log Started: {DateTime.Now} ===", _errorLogFilePath);
        }

        public void LogInformation(string message)
        {
            lock (_logLock)
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}";
                WriteToFile(logMessage, _logFilePath);
                Console.WriteLine(logMessage);
                Debug.WriteLine($"[INFO] {message}");
            }
        }

        public void LogWarning(string message)
        {
            lock (_logLock)
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARNING] {message}";
                WriteToFile(logMessage, _logFilePath);
                WriteToFile(logMessage, _errorLogFilePath);
                Console.WriteLine(logMessage);
                Debug.WriteLine($"[WARNING] {message}");
            }
        }

        public void LogError(string message, Exception? ex = null)
        {
            lock (_logLock)
            {
                var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {message}";
                if (ex != null)
                {
                    errorMessage += $"{Environment.NewLine}Exception: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}";
                }

                WriteToFile(errorMessage, _errorLogFilePath);
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
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DEBUG] {message}";
                WriteToFile(logMessage, _logFilePath);
                Console.WriteLine(logMessage);
                Debug.WriteLine($"[DEBUG] {message}");
            }
        }

        public void LogApiRequest(string serviceName, string url, Dictionary<string, string>? parameters = null)
        {
            lock (_logLock)
            {
                var message = $"[API REQUEST] {serviceName}";
                if (!string.IsNullOrEmpty(url))
                {
                    message += $" - {url}";
                }
                else
                {
                    message += " - URL not specified";
                }

                if (parameters != null && parameters.Count > 0)
                {
                    message += $" | Params: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}";
                }

                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DEBUG] {message}";
                WriteToFile(logMessage, _logFilePath);
                Console.WriteLine(logMessage);
                Debug.WriteLine($"[DEBUG] {message}");
            }
        }

        public void LogApiResponse(string serviceName, int statusCode, string responseContent, TimeSpan duration, Dictionary<string, string>? parameters = null)
        {
            lock (_logLock)
            {
                var truncatedResponse = responseContent?.Length > 200 ? responseContent.Substring(0, 200) + "...[TRUNCATED]" : responseContent ?? "[NULL]";
                var statusText = GetHttpStatusText(statusCode);
                var message = $"[API RESPONSE] {serviceName} - Status: {statusCode} ({statusText}) | Duration: {duration.TotalMilliseconds:F0}ms | Content Length: {responseContent?.Length ?? 0} chars";

                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DEBUG] {message}";
                WriteToFile(logMessage, _logFilePath);

                // Log full response separate file for large responses
                if (responseContent?.Length > 200)
                {
                    var responseFile = Path.Combine(_logDirectory, $"api_response_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    WriteToFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {serviceName} Full Response:{Environment.NewLine}{responseContent}", responseFile);
                }

                // Also write to console
                Console.WriteLine(logMessage);
                Debug.WriteLine($"[DEBUG] {message}");

                // Log full response for errors (4xx, 5xx)
                if (statusCode >= 400)
                {
                    var errorLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {serviceName} API Error Response:{Environment.NewLine}{responseContent}";
                    WriteToFile(errorLog, _errorLogFilePath);
                }
            }
        }

        public void LogApiError(string serviceName, string url, Exception ex, TimeSpan duration)
        {
            lock (_logLock)
            {
                var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [API ERROR] {serviceName}";
                if (!string.IsNullOrEmpty(url))
                {
                    errorMessage += $" | URL: {url}";
                }
                errorMessage += $" | Duration: {duration.TotalMilliseconds:F0}ms | Error: {ex.GetType().Name}: {ex.Message}";

                WriteToFile(errorMessage, _errorLogFilePath);
                Console.Error.WriteLine(errorMessage);
                Debug.WriteLine(errorMessage);

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
                WriteToFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] Error logs cleared by user", _logFilePath);
            }
        }

        public string GetLogDirectory()
        {
            return _logDirectory;
        }

        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        public string GetErrorLogFilePath()
        {
            return _errorLogFilePath;
        }

        private void WriteToFile(string message, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                // If we can't write to file, write to console as last resort
                Console.Error.WriteLine($"Failed to write to log file {filePath}: {ex.Message}");
            }
        }

        private string GetHttpStatusText(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                202 => "Accepted",
                204 => "No Content",
                301 => "Moved Permanently",
                302 => "Found",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                _ => $"Status {statusCode}"
            };
        }
    }

    
}