import React, { useState } from 'react'
import { format } from 'date-fns'
import { Table, ChevronUp, ChevronDown } from 'lucide-react'
import { WeatherData } from '../types/weather'

interface WeatherDataTableProps {
  data: WeatherData[]
}

type SortField = 'date' | 'temperatureMax' | 'temperatureMin' | 'precipitation'
type SortDirection = 'asc' | 'desc'

export function WeatherDataTable({ data }: WeatherDataTableProps) {
  const [sortField, setSortField] = useState<SortField>('date')
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc')

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('asc')
    }
  }

  const sortedData = [...data].sort((a, b) => {
    let aValue: any = a[sortField]
    let bValue: any = b[sortField]
    
    if (sortField === 'date') {
      aValue = new Date(aValue).getTime()
      bValue = new Date(bValue).getTime()
    }
    
    if (sortDirection === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })

  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortField !== field) return null
    return sortDirection === 'asc' ? 
      <ChevronUp className="w-4 h-4" /> : 
      <ChevronDown className="w-4 h-4" />
  }

  return (
    <div className="card">
      <div className="flex items-center gap-2 mb-6">
        <Table className="w-5 h-5 text-primary-600" />
        <h3 className="text-lg font-semibold text-gray-900">Weather Data Details</h3>
        <span className="text-sm text-gray-500">({data.length} records)</span>
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-gray-200">
              <th 
                className="text-left py-3 px-4 font-medium text-gray-700 cursor-pointer hover:bg-gray-50 transition-colors"
                onClick={() => handleSort('date')}
              >
                <div className="flex items-center gap-1">
                  Date
                  <SortIcon field="date" />
                </div>
              </th>
              <th 
                className="text-left py-3 px-4 font-medium text-gray-700 cursor-pointer hover:bg-gray-50 transition-colors"
                onClick={() => handleSort('temperatureMax')}
              >
                <div className="flex items-center gap-1">
                  Max Temp (°C)
                  <SortIcon field="temperatureMax" />
                </div>
              </th>
              <th 
                className="text-left py-3 px-4 font-medium text-gray-700 cursor-pointer hover:bg-gray-50 transition-colors"
                onClick={() => handleSort('temperatureMin')}
              >
                <div className="flex items-center gap-1">
                  Min Temp (°C)
                  <SortIcon field="temperatureMin" />
                </div>
              </th>
              <th 
                className="text-left py-3 px-4 font-medium text-gray-700 cursor-pointer hover:bg-gray-50 transition-colors"
                onClick={() => handleSort('precipitation')}
              >
                <div className="flex items-center gap-1">
                  Precipitation (mm)
                  <SortIcon field="precipitation" />
                </div>
              </th>
              <th className="text-left py-3 px-4 font-medium text-gray-700">
                Provider
              </th>
            </tr>
          </thead>
          <tbody>
            {sortedData.map((item, index) => (
              <tr 
                key={`${item.date}-${index}`}
                className="border-b border-gray-100 hover:bg-gray-50 transition-colors"
              >
                <td className="py-3 px-4 text-sm text-gray-900">
                  {format(new Date(item.date), 'MMM dd, yyyy')}
                </td>
                <td className="py-3 px-4 text-sm">
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                    {item.temperatureMax.toFixed(1)}°C
                  </span>
                </td>
                <td className="py-3 px-4 text-sm">
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    {item.temperatureMin.toFixed(1)}°C
                  </span>
                </td>
                <td className="py-3 px-4 text-sm">
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-cyan-100 text-cyan-800">
                    {item.precipitation.toFixed(1)}mm
                  </span>
                </td>
                <td className="py-3 px-4 text-sm text-gray-600">
                  {item.weatherProvider}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}