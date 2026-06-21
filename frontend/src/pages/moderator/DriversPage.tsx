import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { driversApi } from '@/api/drivers'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { Search } from 'lucide-react'

export function ModeratorDriversPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['drivers', search],
    queryFn: () => driversApi.getAll({ search, pageSize: 50 }),
  })

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Drivers</h1>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
        <Input
          className="pl-9"
          placeholder="Search by name, plate, or phone…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}

      <div className="space-y-3">
        {data?.items.map((driver) => (
          <Card key={driver.id}>
            <CardContent className="pt-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-1">
                    <h3 className="font-semibold text-gray-900 dark:text-white">{driver.driverName}</h3>
                    <RiskBadge status={driver.status} />
                  </div>
                  <p className="text-sm text-gray-500">{driver.phoneNumber}</p>
                  {driver.vehicles?.[0] && (
                    <p className="text-sm text-gray-500">
                      {driver.vehicles[0].registrationNumber} · {driver.vehicles[0].make} {driver.vehicles[0].model}
                    </p>
                  )}
                  <p className="text-xs text-gray-400 mt-1">{driver.reportCount} reports</p>
                </div>
                <div className="w-40">
                  <RiskScore score={driver.riskScore} />
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
