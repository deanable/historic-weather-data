import React from 'react'
import { Calendar } from 'lucide-react'
import { format } from 'date-fns'
import { WeatherQueryParams } from '../types/weather'

interface DateRangeSelectorProps {
  params: WeatherQueryParams
  onChange: (updates: Partial<WeatherQueryParams>) => void
}

const TIME_RANGES = [
  { value: '1 Day', label: '1 Day', days: 1 },
  { value: '1 Week', label: '1 Week', days: 7 },
  { value: '14 Days', label: '2 Weeks', days: 14 },
  { value: '30 Days', label: '1 Month', days: 30 },
  { value: '3 Months', label: '3 Months', days: 90 },
  { value: '6 Months', label: '6 Months', days: 180 },
  { value: '12 Months', label: '1 Year', days: 365 },
  { value: 'Custom Range', label: 'Custom Range', days: 0 },
]

export function DateRangeSelector({ params, onChange }: DateRangeSelectorProps) {
  const handleTimeRangeChange = (timeRange: string) => {
    const range = TIME_RANGES.find(r => r.value === timeRange)
    if (!range) return

    onChange({ timeRange })

    if (range.days > 0) {
      const endDate = new Date()
      const startDate = new Date(endDate.getTime() - range.days * 24 * 60 * 60 * 1000)
      onChange({ startDate, endDate })
    }
  }

  return (
    <div className="card">
      <div className="flex items-center gap-2 mb-4">
        <Calendar className="w-5 h-5 text-primary-600" />
        <h3 className="text-lg font-semibold text-gray-900">Date Range</h3>
      </div>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Preset Ranges
          </label>
          <div className="grid grid-cols-2 gap-2">
            {TIME_RANGES.slice(0, -1).map((range) => (
              <button
                key={range.value}
                onClick={() => handleTimeRangeChange(range.value)}
                className={`px-3 py-2 text-sm rounded-lg border transition-colors ${
                  params.timeRange === range.value
                    ? 'bg-primary-600 text-white border-primary-600'
                    : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                }`}
              >
                {range.label}
              </button>
            ))}
          </div>
        </div>
        
        <div>
          <button
            onClick={() => handleTimeRangeChange('Custom Range')}
            className={`w-full px-3 py-2 text-sm rounded-lg border transition-colors ${
              params.timeRange === 'Custom Range'
                ? 'bg-primary-600 text-white border-primary-600'
                : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
            }`}
          >
            Custom Range
          </button>
        </div>
        
        {params.timeRange === 'Custom Range' && (
          <div className="grid grid-cols-2 gap-3 pt-2 border-t border-gray-200">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Start Date
              </label>
              <input
                type="date"
                value={format(params.startDate, 'yyyy-MM-dd')}
                onChange={(e) => onChange({ startDate: new Date(e.target.value) })}
                className="input-field"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                End Date
              </label>
              <input
                type="date"
                value={format(params.endDate, 'yyyy-MM-dd')}
                onChange={(e) => onChange({ endDate: new Date(e.target.value) })}
                className="input-field"
              />
            </div>
          </div>
        )}
      </div>
    </div>
  )
}