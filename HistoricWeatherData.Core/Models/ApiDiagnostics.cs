using System.Diagnostics;

namespace HistoricWeatherData.Core.Models
{
    public class ApiDiagnostics
    {
        private readonly Stopwatch _stopwatch;

        public ApiDiagnostics(string serviceName, string operationName)
        {
            ServiceName = serviceName;
            OperationName = operationName;
            StartTime = DateTime.Now;
            _stopwatch = Stopwatch.StartNew();
        }

        public string ServiceName { get; }
        public string OperationName { get; }
        public DateTime StartTime { get; }
        public TimeSpan? Duration { get; private set; }
        public bool? IsSuccess { get; private set; }
        public int? StatusCode { get; private set; }
        public int RequestCount { get; private set; }
        public int ErrorCount { get; private set; }
        public List<string> Errors { get; } = new();

        public void AddRequest()
        {
            RequestCount++;
        }

        public void AddError(string errorMessage)
        {
            ErrorCount++;
            Errors.Add(errorMessage);
        }

        public void SetStatusCode(int statusCode)
        {
            StatusCode = statusCode;
        }

        public void Complete(bool isSuccess = false, int? statusCode = null)
        {
            _stopwatch.Stop();
            Duration = _stopwatch.Elapsed;
            IsSuccess = isSuccess;
            if (statusCode.HasValue)
            {
                StatusCode = statusCode.Value;
            }
        }

        public void AddError(Exception ex)
        {
            AddError($"{ex.GetType().Name}: {ex.Message}");
        }

        public string GetSummary()
        {
            var successStr = IsSuccess.HasValue ? (IsSuccess.Value ? "SUCCESS" : "FAILED") : "UNKNOWN";
            return $"{ServiceName}.{OperationName}: {successStr} ({Duration?.TotalSeconds:F2}s) - {RequestCount} requests, {ErrorCount} errors";
        }
    }
}