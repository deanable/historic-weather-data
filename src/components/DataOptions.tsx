import React from 'react'
import { Settings, Cloud } from 'lucide-react'
import { WeatherQueryParams } from '../types/weather'

interface DataOptionsProps {
  params: WeatherQueryParams
  onChange: (updates: Partial<WeatherQueryParams>) => void
}

const WEATHER_PROVIDERS = [
  { value: 'OpenMeteo', label: 'Open-Meteo (Free)', requiresKey: false },
  { value: 'Visual Crossing', label: 'Visual Crossing', requiresKey: true },
  { value: 'OpenWeatherMap', label: 'OpenWeatherMap', requiresKey: true },
  { value: 'WeatherAPI', label: 'WeatherAPI', requiresKey: true },
]

const YEARS_OPTIONS = Array.from({ length: 10 }, (_, i) => i + 1)

export function DataOptions({ params, onChange }: DataOptionsProps) {
  return (
    <div className="card">
      <div className="flex items-center gap-2 mb-4">
        <Settings className="w-5 h-5 text-primary-600" />
        <h3 className="text-lg font-semibold text-gray-900">Data Options</h3>
      </div>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Weather Provider
          </label>
          <select
            value={params.provider}
            onChange={(e) => onChange({ provider: e.target.value })}
            className="select-field"
          >
            {WEATHER_PROVIDERS.map((provider) => (
              <option key={provider.value} value={provider.value}>
                {provider.label}
              </option>
            ))}
          </select>
          <p className="text-xs text-gray-500 mt-1">
            {WEATHER_PROVIDERS.find(p => p.value === params.provider)?.requiresKey 
              ? 'Requires API key' 
              : 'No API key required'
            }
          </p>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Years of Historical Data
          </label>
          <select
            value={params.yearsBack}
            onChange={(e) => onChange({ yearsBack: parseInt(e.target.value) })}
            className="select-field"
          >
            {YEARS_OPTIONS.map((year) => (
              <option key={year} value={year}>
                {year} {year === 1 ? 'Year' : 'Years'}
              </option>
            ))}
          </select>
        </div>
        
        <div className="flex items-center gap-2 p-3 bg-blue-50 rounded-lg">
          <Cloud className="w-4 h-4 text-blue-600" />
          <p className="text-sm text-blue-700">
            Data fetched from multiple years for comparison
          </p>
        </div>
      </div>
    </div>
  )
}