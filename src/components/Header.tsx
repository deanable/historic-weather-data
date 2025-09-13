import React from 'react'
import { Cloud, BarChart3 } from 'lucide-react'

export function Header() {
  return (
    <header className="bg-white border-b border-gray-200 shadow-sm">
      <div className="container mx-auto px-4 py-6">
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-primary-600">
            <Cloud className="w-8 h-8" />
            <BarChart3 className="w-6 h-6" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Historic Weather Analytics
            </h1>
            <p className="text-gray-600 text-sm">
              Comprehensive weather data analysis and visualization
            </p>
          </div>
        </div>
      </div>
    </header>
  )
}