using HistoricWeatherData.Core.Models;
using HistoricWeatherData.Core.Services.Interfaces;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoricWeatherData.Core.Services.Implementations
{
    public class DataExportService : IDataExportService
    {
        public async Task ExportDataAsCsvAsync(string location, List<WeatherData> weatherData, bool exportAverages)
        {
            var exportPath = Path.Combine(Path.GetTempPath(), location);
            Directory.CreateDirectory(exportPath);

            var yearlyData = weatherData.GroupBy(d => d.Date.Year);

            foreach (var yearGroup in yearlyData)
            {
                var year = yearGroup.Key;
                var filePath = Path.Combine(exportPath, $"{location} {year}.csv");
                var csv = new StringBuilder();
                csv.AppendLine("Date,TemperatureMin,TemperatureMax,Precipitation");

                foreach (var data in yearGroup.OrderBy(d => d.Date))
                {
                    csv.AppendLine($"{data.Date:yyyy-MM-dd},{data.TemperatureMin},{data.TemperatureMax},{data.Precipitation}");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString());
            }

            if (exportAverages)
            {
                var averages = weatherData
                    .GroupBy(d => d.Date.ToString("MM-dd"))
                    .Select(g => new
                    {
                        Date = g.Key,
                        AvgMinTemp = g.Average(d => d.TemperatureMin),
                        AvgMaxTemp = g.Average(d => d.TemperatureMax),
                        AvgPrecip = g.Average(d => d.Precipitation)
                    })
                    .OrderBy(a => a.Date);

                var filePath = Path.Combine(exportPath, $"{location} averages.csv");
                var csv = new StringBuilder();
                csv.AppendLine("Date,AvgMinTemp,AvgMaxTemp,AvgPrecipitation");

                foreach (var avg in averages)
                {
                    csv.AppendLine($"{avg.Date},{avg.AvgMinTemp:F2},{avg.AvgMaxTemp:F2},{avg.AvgPrecip:F2}");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString());
            }

            Debug.WriteLine($"Exported data to {exportPath}");
        }

        public async Task ExportDataAsExcelAsync(string location, List<WeatherData> weatherData, bool exportAverages)
        {
            using (var excelEngine = new Syncfusion.XlsIO.ExcelEngine())
            {
                var application = excelEngine.Excel;
                application.DefaultVersion = Syncfusion.XlsIO.ExcelVersion.Xlsx;
                var workbook = application.Workbooks.Create(1);

                var yearlyData = weatherData.GroupBy(d => d.Date.Year);

                foreach (var yearGroup in yearlyData)
                {
                    var year = yearGroup.Key;
                    var worksheet = workbook.Worksheets.Create($"{location} {year}");
                    worksheet.Range["A1"].Text = "Date";
                    worksheet.Range["B1"].Text = "TemperatureMin";
                    worksheet.Range["C1"].Text = "TemperatureMax";
                    worksheet.Range["D1"].Text = "Precipitation";

                    var row = 2;
                    foreach (var data in yearGroup.OrderBy(d => d.Date))
                    {
                        worksheet.Range[row, 1].Text = data.Date.ToString("yyyy-MM-dd");
                        worksheet.Range[row, 2].Number = data.TemperatureMin;
                        worksheet.Range[row, 3].Number = data.TemperatureMax;
                        worksheet.Range[row, 4].Number = data.Precipitation;
                        row++;
                    }
                }

                if (exportAverages)
                {
                    var averages = weatherData
                        .GroupBy(d => d.Date.ToString("MM-dd"))
                        .Select(g => new
                        {
                            Date = g.Key,
                            AvgMinTemp = g.Average(d => d.TemperatureMin),
                            AvgMaxTemp = g.Average(d => d.TemperatureMax),
                            AvgPrecip = g.Average(d => d.Precipitation)
                        })
                        .OrderBy(a => a.Date);

                    var worksheet = workbook.Worksheets.Create($"{location} Averages");
                    worksheet.Range["A1"].Text = "Date";
                    worksheet.Range["B1"].Text = "AvgMinTemp";
                    worksheet.Range["C1"].Text = "AvgMaxTemp";
                    worksheet.Range["D1"].Text = "AvgPrecipitation";

                    var row = 2;
                    foreach (var avg in averages)
                    {
                        worksheet.Range[row, 1].Text = avg.Date;
                        worksheet.Range[row, 2].Number = avg.AvgMinTemp;
                        worksheet.Range[row, 3].Number = avg.AvgMaxTemp;
                        worksheet.Range[row, 4].Number = avg.AvgPrecip;
                        row++;
                    }
                }

                var exportPath = Path.Combine(Path.GetTempPath(), location);
                Directory.CreateDirectory(exportPath);
                var filePath = Path.Combine(exportPath, $"{location}.xlsx");

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    await File.WriteAllBytesAsync(filePath, stream.ToArray());
                }

                Debug.WriteLine($"Exported data to {filePath}");
            }
        }
    }
}

