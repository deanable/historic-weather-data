using CommunityToolkit.Maui.Storage;
using HistoricWeatherData.Core.Services.Interfaces;
using Syncfusion.Maui.Charts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HistoricWeatherData.Services
{
    public class ChartExportService : IChartExportService
    {
        public async Task ExportChartAsync(object chart)
        {
            if (chart is not SfCartesianChart cartesianChart)
            {
                return;
            }

            using var stream = await cartesianChart.GetStreamAsync();
            var fileSaverResult = await FileSaver.Default.SaveAsync("WeatherDataChart.png", stream, CancellationToken.None);

            if (fileSaverResult.IsSuccessful)
            {
                // You could use the CommunityToolkit's Snackbar to show a success message
                // For now, we'll just complete silently.
            }
            else
            {
                // You could show an error message
            }
        }
    }
}
