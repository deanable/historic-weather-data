import React from 'react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts'
import { format } from 'date-fns'
import { TrendingUp } from 'lucide-react'
import { WeatherData } from '../types/weather'

interface WeatherChartProps {
  data: WeatherData[]
}

export function WeatherChart({ data }: WeatherChartProps) {
  const chartData = data.map(item => ({
    date: format(new Date(item.date), 'MMM dd'),
    fullDate: format(new Date(item.date), 'yyyy-MM-dd'),
    maxTemp: Math.round(item.temperatureMax * 10) / 10,
    minTemp: Math.round(item.temperatureMin * 10) / 10,
    precipitation: Math.round(item.precipitation * 10) / 10,
  }))

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload
      return (
        <div className="bg-white p-4 border border-gray-200 rounded-lg shadow-lg">
          <p className="font-medium text-gray-900 mb-2">{data.fullDate}</p>
          {payload.map((entry: any, index: number) => (
            <p key={index} className="text-sm" style={{ color: entry.color }}>
              {entry.name}: {entry.value}
              {entry.dataKey.includes('Temp') ? '°C' : 'mm'}
            </p>
          ))}
        </div>
      )
    }
    return null
  }

  return (
    <div className="card">
      <div className="flex items-center gap-2 mb-6">
        <TrendingUp className="w-5 h-5 text-primary-600" />
        <h3 className="text-lg font-semibold text-gray-900">Weather Trends</h3>
        <span className="text-sm text-gray-500">({data.length} data points)</span>
      </div>
      
      <div className="space-y-8">
        {/* Temperature Chart */}
        <div>
          <h4 className="text-md font-medium text-gray-800 mb-4">Temperature (°C)</h4>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis 
                dataKey="date" 
                stroke="#6b7280"
                fontSize={12}
                tickLine={false}
              />
              <YAxis 
                stroke="#6b7280"
                fontSize={12}
                tickLine={false}
              />
              <Tooltip content={<CustomTooltip />} />
              <Legend />
              <Line 
                type="monotone" 
                dataKey="maxTemp" 
                stroke="#ef4444" 
                strokeWidth={2}
                name="Max Temperature"
                dot={{ fill: '#ef4444', strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: '#ef4444', strokeWidth: 2 }}
              />
              <Line 
                type="monotone" 
                dataKey="minTemp" 
                stroke="#3b82f6" 
                strokeWidth={2}
                name="Min Temperature"
                dot={{ fill: '#3b82f6', strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: '#3b82f6', strokeWidth: 2 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Precipitation Chart */}
        <div>
          <h4 className="text-md font-medium text-gray-800 mb-4">Precipitation (mm)</h4>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis 
                dataKey="date" 
                stroke="#6b7280"
                fontSize={12}
                tickLine={false}
              />
              <YAxis 
                stroke="#6b7280"
                fontSize={12}
                tickLine={false}
              />
              <Tooltip content={<CustomTooltip />} />
              <Legend />
              <Bar 
                dataKey="precipitation" 
                fill="#06b6d4" 
                name="Precipitation"
                radius={[2, 2, 0, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  )
}