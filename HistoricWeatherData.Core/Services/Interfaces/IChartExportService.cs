using System.Threading.Tasks;

namespace HistoricWeatherData.Core.Services.Interfaces
{
    public interface IChartExportService
    {
        Task ExportChartAsync(object chart);
    }
}
