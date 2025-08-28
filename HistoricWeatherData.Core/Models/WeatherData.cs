using System;

namespace HistoricWeatherData.Core.Models
{
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public double TemperatureMin { get; set; }
        public double TemperatureMax { get; set; }
        public double Precipitation { get; set; }
        public string WeatherProvider { get; set; } = string.Empty;
        public LocationData Location { get; set; } = new LocationData();
    }

    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class WeatherQueryParameters
    {
        public LocationData Location { get; set; } = new LocationData();
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int YearsBack { get; set; } = 1;
        public string ProviderName { get; set; } = string.Empty;
    }

    public class WeatherResponse
    {
        public List<WeatherData> Data { get; set; } = new List<WeatherData>();
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}