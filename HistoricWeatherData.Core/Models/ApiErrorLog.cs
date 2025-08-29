namespace HistoricWeatherData.Core.Models
{
    public class ApiErrorLog
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ServiceName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public int? StatusCode { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public Dictionary<string, string> RequestParameters { get; set; } = new();
    }
}