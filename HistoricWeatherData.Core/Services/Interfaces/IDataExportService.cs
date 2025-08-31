using HistoricWeatherData.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IDataExportService
    {
        Task ExportDataAsCsvAsync(string location, List<WeatherData> weatherData, bool exportAverages);
        Task ExportDataAsExcelAsync(string location, List<WeatherData> weatherData, bool exportAverages);
    }
}
