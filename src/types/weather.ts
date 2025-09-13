export interface WeatherData {
  date: string
  temperatureMin: number
  temperatureMax: number
  precipitation: number
  weatherProvider: string
  location: LocationData
}

export interface LocationData {
  latitude: number
  longitude: number
  cityName: string
  country: string
  state: string
}

export interface WeatherQueryParams {
  latitude: number
  longitude: number
  locationName: string
  startDate: Date
  endDate: Date
  yearsBack: number
  provider: string
  timeRange: string
}

export interface WeatherResponse {
  data: WeatherData[]
  isSuccess: boolean
  errorMessage?: string
}

export interface ApiError {
  message: string
  statusCode?: number
  provider?: string
}