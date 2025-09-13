import { useQuery } from '@tanstack/react-query'
import { WeatherQueryParams, WeatherData } from '../types/weather'
import { weatherService } from '../services/weatherService'

export function useWeatherData(params: WeatherQueryParams) {
  return useQuery({
    queryKey: ['weather-data', params],
    queryFn: () => weatherService.getHistoricalWeatherData(params),
    enabled: false, // Only fetch when explicitly triggered
    staleTime: 10 * 60 * 1000, // 10 minutes
    retry: (failureCount, error: any) => {
      // Don't retry on authentication errors
      if (error?.statusCode === 401 || error?.statusCode === 403) {
        return false
      }
      return failureCount < 2
    },
  })
}