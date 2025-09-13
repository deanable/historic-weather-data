import React from 'react'
import { MapPin, Navigation } from 'lucide-react'
import { WeatherQueryParams } from '../types/weather'

interface LocationInputProps {
  params: WeatherQueryParams
  onChange: (updates: Partial<WeatherQueryParams>) => void
}

export function LocationInput({ params, onChange }: LocationInputProps) {
  const handleGetCurrentLocation = async () => {
    if (!navigator.geolocation) {
      alert('Geolocation is not supported by this browser.')
      return
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        onChange({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          locationName: `${position.coords.latitude.toFixed(4)}, ${position.coords.longitude.toFixed(4)}`
        })
      },
      (error) => {
        console.error('Error getting location:', error)
        alert('Unable to get your current location.')
      }
    )
  }

  return (
    <div className="card">
      <div className="flex items-center gap-2 mb-4">
        <MapPin className="w-5 h-5 text-primary-600" />
        <h3 className="text-lg font-semibold text-gray-900">Location</h3>
      </div>
      
      <div className="space-y-4">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Latitude
            </label>
            <input
              type="number"
              step="0.000001"
              value={params.latitude}
              onChange={(e) => onChange({ latitude: parseFloat(e.target.value) || 0 })}
              className="input-field"
              placeholder="40.7128"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Longitude
            </label>
            <input
              type="number"
              step="0.000001"
              value={params.longitude}
              onChange={(e) => onChange({ longitude: parseFloat(e.target.value) || 0 })}
              className="input-field"
              placeholder="-74.0060"
            />
          </div>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Location Name
          </label>
          <input
            type="text"
            value={params.locationName}
            onChange={(e) => onChange({ locationName: e.target.value })}
            className="input-field"
            placeholder="New York, NY"
          />
        </div>
        
        <button
          onClick={handleGetCurrentLocation}
          className="w-full btn-secondary flex items-center justify-center gap-2"
        >
          <Navigation className="w-4 h-4" />
          Get Current Location
        </button>
      </div>
    </div>
  )
}