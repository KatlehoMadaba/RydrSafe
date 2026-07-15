import { useQuery } from '@tanstack/react-query'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import { RISK_PRESENTATION } from '@/lib/riskStatus'
import type { DriverStatus } from '@/types'

const CATEGORIES = ['RecklessDriving', 'Harassment', 'Assault', 'Theft', 'Fraud', 'UnsafeVehicle', 'IntoxicatedDriving', 'Other']
const DRIVER_STATUSES: DriverStatus[] = ['Safe', 'UnderReview', 'Flagged', 'HighRisk']

export function AdminAnalyticsPage() {
  const { data: reports } = useQuery({ queryKey: ['all-reports'], queryFn: () => reportsApi.getAll({ pageSize: 500 }) })
  const { data: drivers } = useQuery({ queryKey: ['all-drivers'], queryFn: () => driversApi.getAll({ pageSize: 500 }) })

  const categoryCounts = CATEGORIES.map((cat) => ({
    category: cat.replace(/([A-Z])/g, ' $1').trim(),
    count: reports?.items.filter(r => r.category === cat).length ?? 0,
  })).sort((a, b) => b.count - a.count)

  const statusCounts = DRIVER_STATUSES.map((status) => ({
    status,
    label: RISK_PRESENTATION[status].label,
    count: drivers?.items.filter((d) => d.status === status).length ?? 0,
  }))

  const topRisk = (drivers?.items ?? []).sort((a, b) => b.riskScore - a.riskScore).slice(0, 5)

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Analytics</h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        {statusCounts.map(({ status, label, count }) => (
          <Card key={status}>
            <CardContent className="pt-6 text-center">
              <p className="text-3xl font-bold text-foreground">{count}</p>
              <p className="text-sm text-muted-foreground mt-1">{label}</p>
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
                      <span className="text-foreground">{category}</span>
                      <span className="font-medium text-foreground">{count}</span>
                    </div>
                    <div className="h-2 bg-muted rounded-full">
                      <div className="h-2 bg-teal-500 rounded-full transition-all" style={{ width: `${(count / max) * 100}%` }} />
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
                    <span className="text-sm font-medium text-foreground">{driver.driverName}</span>
                    <RiskBadge state={driver.status} />
                  </div>
                  <RiskScore score={driver.riskScore} />
                </div>
              ))}
              {topRisk.length === 0 && <p className="text-sm text-muted-foreground text-center py-4">No driver data available</p>}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
