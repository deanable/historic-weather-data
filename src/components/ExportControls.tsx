import React, { useState } from 'react'
import { Download, FileText, FileSpreadsheet } from 'lucide-react'
import { format } from 'date-fns'
import { WeatherData } from '../types/weather'
import { exportToCSV, exportToJSON } from '../utils/exportUtils'

interface ExportControlsProps {
  weatherData: WeatherData[]
  locationName: string
  disabled?: boolean
}

export function ExportControls({ weatherData, locationName, disabled }: ExportControlsProps) {
  const [isExporting, setIsExporting] = useState(false)

  const handleExport = async (format: 'csv' | 'json') => {
    if (weatherData.length === 0) return

    setIsExporting(true)
    try {
      const filename = `${locationName.replace(/[^a-zA-Z0-9]/g, '_')}_weather_data_${format(new Date(), 'yyyy-MM-dd')}`
      
      if (format === 'csv') {
        exportToCSV(weatherData, filename)
      } else {
        exportToJSON(weatherData, filename)
      }
    } catch (error) {
      console.error('Export failed:', error)
      alert('Export failed. Please try again.')
    } finally {
      setIsExporting(false)
    }
  }

  return (
    <div className="flex gap-2">
      <button
        onClick={() => handleExport('csv')}
        disabled={disabled || isExporting}
        className="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
      >
        <FileSpreadsheet className="w-4 h-4" />
        Export CSV
      </button>
      
      <button
        onClick={() => handleExport('json')}
        disabled={disabled || isExporting}
        className="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
      >
        <FileText className="w-4 h-4" />
        Export JSON
      </button>
    </div>
  )
}