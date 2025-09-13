import { WeatherQueryParams, WeatherData, WeatherResponse, ApiError } from '../types/weather'

class WeatherService {
  private baseUrl = 'https://archive-api.open-meteo.com/v1/archive'

  async getHistoricalWeatherData(params: WeatherQueryParams): Promise<WeatherData[]> {
    try {
      const allData: WeatherData[] = []
      const currentYear = new Date().getFullYear()
      const startYear = currentYear - params.yearsBack

      for (let year = startYear; year <= currentYear; year++) {
        const yearData = await this.fetchYearData(params, year)
        allData.push(...yearData)
      }

      return allData
    } catch (error) {
      console.error('Weather service error:', error)
      throw new Error(error instanceof Error ? error.message : 'Failed to fetch weather data')
    }
  }

  private async fetchYearData(params: WeatherQueryParams, year: number): Promise<WeatherData[]> {
    const startDate = new Date(year, params.startDate.getMonth(), params.startDate.getDate())
    const endDate = new Date(year, params.endDate.getMonth(), params.endDate.getDate())

    // Skip future dates
    if (year === new Date().getFullYear() && startDate > new Date()) {
      return []
    }

    const url = new URL(this.baseUrl)
    url.searchParams.set('latitude', params.latitude.toString())
    url.searchParams.set('longitude', params.longitude.toString())
    url.searchParams.set('start_date', this.formatDate(startDate))
    url.searchParams.set('end_date', this.formatDate(endDate))
    url.searchParams.set('daily', 'temperature_2m_max,temperature_2m_min,precipitation_sum')
    url.searchParams.set('timezone', 'auto')

    const response = await fetch(url.toString())
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }

    const result = await response.json()
    
    if (!result.daily?.time) {
      return []
    }

    return result.daily.time.map((date: string, index: number) => ({
      date,
      temperatureMin: result.daily.temperature_2m_min[index] || 0,
      temperatureMax: result.daily.temperature_2m_max[index] || 0,
      precipitation: result.daily.precipitation_sum[index] || 0,
      weatherProvider: 'Open-Meteo',
      location: {
        latitude: params.latitude,
        longitude: params.longitude,
        cityName: params.locationName,
        country: 'Unknown',
        state: 'Unknown'
      }
    }))
  }

  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0]
  }
}

export const weatherService = new WeatherService()