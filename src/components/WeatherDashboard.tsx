import React, { useState } from 'react'
import { LocationInput } from './LocationInput'
import { DataOptions } from './DataOptions'
import { DateRangeSelector } from './DateRangeSelector'
import { WeatherChart } from './WeatherChart'
import { WeatherDataTable } from './WeatherDataTable'
import { ExportControls } from './ExportControls'
import { StatusMessage } from './StatusMessage'
import { useWeatherData } from '../hooks/useWeatherData'
import { WeatherQueryParams } from '../types/weather'

export function WeatherDashboard() {
  const [queryParams, setQueryParams] = useState<WeatherQueryParams>({
    latitude: 40.7128,
    longitude: -74.0060,
    locationName: 'New York, NY',
    startDate: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), // 7 days ago
    endDate: new Date(),
    yearsBack: 1,
    provider: 'OpenMeteo',
    timeRange: '1 Week'
  })

  const { data: weatherData, isLoading, error, refetch } = useWeatherData(queryParams)

  const handleLoadData = () => {
    refetch()
  }

  const handleParamsChange = (updates: Partial<WeatherQueryParams>) => {
    setQueryParams(prev => ({ ...prev, ...updates }))
  }

  return (
    <div className="space-y-8 animate-fade-in">
      {/* Configuration Section */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <LocationInput 
          params={queryParams}
          onChange={handleParamsChange}
        />
        <DataOptions 
          params={queryParams}
          onChange={handleParamsChange}
        />
        <DateRangeSelector 
          params={queryParams}
          onChange={handleParamsChange}
        />
      </div>

      {/* Action Controls */}
      <div className="flex flex-wrap gap-4 justify-center">
        <button
          onClick={handleLoadData}
          disabled={isLoading}
          className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          {isLoading ? (
            <>
              <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              Loading...
            </>
          ) : (
            'Load Weather Data'
          )}
        </button>
        
        <ExportControls 
          weatherData={weatherData || []}
          locationName={queryParams.locationName}
          disabled={isLoading || !weatherData?.length}
        />
      </div>

      {/* Status */}
      <StatusMessage 
        isLoading={isLoading}
        error={error}
        dataCount={weatherData?.length || 0}
      />

      {/* Results Section */}
      {weatherData && weatherData.length > 0 && (
        <div className="space-y-8 animate-slide-up">
          <WeatherChart data={weatherData} />
          <WeatherDataTable data={weatherData} />
        </div>
      )}
    </div>
  )
}