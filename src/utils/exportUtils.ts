import { format } from 'date-fns'
import { WeatherData } from '../types/weather'

export function exportToCSV(data: WeatherData[], filename: string) {
  const headers = ['Date', 'Max Temperature (°C)', 'Min Temperature (°C)', 'Precipitation (mm)', 'Provider', 'Location']
  
  const csvContent = [
    headers.join(','),
    ...data.map(item => [
      format(new Date(item.date), 'yyyy-MM-dd'),
      item.temperatureMax.toFixed(2),
      item.temperatureMin.toFixed(2),
      item.precipitation.toFixed(2),
      item.weatherProvider,
      item.location.cityName
    ].join(','))
  ].join('\n')

  downloadFile(csvContent, `${filename}.csv`, 'text/csv')
}

export function exportToJSON(data: WeatherData[], filename: string) {
  const jsonContent = JSON.stringify({
    exportDate: new Date().toISOString(),
    recordCount: data.length,
    data: data.map(item => ({
      ...item,
      date: format(new Date(item.date), 'yyyy-MM-dd')
    }))
  }, null, 2)

  downloadFile(jsonContent, `${filename}.json`, 'application/json')
}

function downloadFile(content: string, filename: string, mimeType: string) {
  const blob = new Blob([content], { type: mimeType })
  const url = URL.createObjectURL(blob)
  
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}