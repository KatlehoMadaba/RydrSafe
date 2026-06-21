import { useQuery } from '@tanstack/react-query'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { RiskScore } from '@/components/RiskScore'

const CATEGORIES = ['RecklessDriving', 'Harassment', 'Assault', 'Theft', 'Fraud', 'UnsafeVehicle', 'IntoxicatedDriving', 'Other']

export function AdminAnalyticsPage() {
  const { data: reports } = useQuery({ queryKey: ['all-reports'], queryFn: () => reportsApi.getAll({ pageSize: 500 }) })
  const { data: drivers } = useQuery({ queryKey: ['all-drivers'], queryFn: () => driversApi.getAll({ pageSize: 500 }) })

  const categoryCounts = CATEGORIES.map((cat) => ({
    category: cat.replace(/([A-Z])/g, ' $1').trim(),
    count: reports?.items.filter(r => r.category === cat).length ?? 0,
  })).sort((a, b) => b.count - a.count)

  const statusCounts = {
    Safe: drivers?.items.filter(d => d.status === 'Safe').length ?? 0,
    UnderReview: drivers?.items.filter(d => d.status === 'UnderReview').length ?? 0,
    Flagged: drivers?.items.filter(d => d.status === 'Flagged').length ?? 0,
    HighRisk: drivers?.items.filter(d => d.status === 'HighRisk').length ?? 0,
  }

  const topRisk = (drivers?.items ?? []).sort((a, b) => b.riskScore - a.riskScore).slice(0, 5)

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Analytics</h1>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {Object.entries(statusCounts).map(([status, count]) => (
          <Card key={status}>
            <CardContent className="pt-6 text-center">
              <p className="text-3xl font-bold text-gray-900 dark:text-white">{count}</p>
              <p className="text-sm text-gray-500 mt-1">{status.replace(/([A-Z])/g, ' $1').trim()}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader><CardTitle className="text-base">Reports by Category</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-3">
              {categoryCounts.map(({ category, count }) => {
                const max = Math.max(...categoryCounts.map(c => c.count), 1)
                return (
                  <div key={category}>
                    <div className="flex items-center justify-between text-sm mb-1">
                      <span className="text-gray-700 dark:text-gray-300">{category}</span>
                      <span className="font-medium text-gray-900 dark:text-white">{count}</span>
                    </div>
                    <div className="h-2 bg-gray-100 dark:bg-gray-800 rounded-full">
                      <div
                        className="h-2 bg-blue-500 rounded-full transition-all"
                        style={{ width: `${(count / max) * 100}%` }}
                      />
                    </div>
                  </div>
                )
              })}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-base">Top 5 Highest Risk Drivers</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-4">
              {topRisk.map((driver) => (
                <div key={driver.id}>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-sm font-medium text-gray-900 dark:text-white">{driver.driverName}</span>
                    <Badge variant={driver.status === 'HighRisk' ? 'destructive' : driver.status === 'Flagged' ? 'warning' : 'secondary'}>
                      {driver.status}
                    </Badge>
                  </div>
                  <RiskScore score={driver.riskScore} />
                </div>
              ))}
              {topRisk.length === 0 && <p className="text-sm text-gray-500 text-center py-4">No driver data available</p>}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
