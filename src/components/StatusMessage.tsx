import React from 'react'
import { AlertCircle, CheckCircle, Loader2 } from 'lucide-react'

interface StatusMessageProps {
  isLoading: boolean
  error: Error | null
  dataCount: number
}

export function StatusMessage({ isLoading, error, dataCount }: StatusMessageProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center gap-2 py-4">
        <Loader2 className="w-5 h-5 animate-spin text-primary-600" />
        <span className="text-gray-600">Loading weather data...</span>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center gap-2 py-4 px-6 bg-error-50 border border-error-200 rounded-lg">
        <AlertCircle className="w-5 h-5 text-error-600" />
        <span className="text-error-700">
          Error: {error.message}
        </span>
      </div>
    )
  }

  if (dataCount > 0) {
    return (
      <div className="flex items-center justify-center gap-2 py-4 px-6 bg-success-50 border border-success-200 rounded-lg">
        <CheckCircle className="w-5 h-5 text-success-600" />
        <span className="text-success-700">
          Successfully loaded {dataCount} weather records
        </span>
      </div>
    )
  }

  return (
    <div className="text-center py-4">
      <span className="text-gray-500">Ready to load weather data</span>
    </div>
  )
}