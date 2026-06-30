import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { FileText, Car, AlertTriangle, Clock } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'
import { Badge } from '@/components/ui/badge'

export function ModeratorDashboardPage() {
  const { data: reports } = useQuery({ queryKey: ['reports'], queryFn: () => reportsApi.getAll({ pageSize: 5 }) })
  const { data: drivers } = useQuery({ queryKey: ['drivers'], queryFn: () => driversApi.getAll({ pageSize: 5 }) })
  const { data: flaggedCount, error: flaggedError, isLoading: flaggedLoading } = useQuery({ queryKey: ['drivers', 'flagged-count'], queryFn: () => driversApi.getFlaggedCount() })

  const pending = reports?.items.filter(r => r.status === 'Pending').length ?? 0
  const flagged = flaggedCount ?? 0
  console.log('[flagged-count] raw:', flaggedCount, '| loading:', flaggedLoading, '| error:', flaggedError)
  const stats = [
    { label: 'Pending Reports', value: pending, icon: Clock, color: 'text-yellow-600 bg-yellow-50' },
    { label: 'Total Reports', value: reports?.totalCount ?? 0, icon: FileText, color: 'text-blue-600 bg-blue-50' },
    { label: 'Flagged Drivers', value: flagged ?? 0, icon: AlertTriangle, color: 'text-red-600 bg-red-50' },
    { label: 'Total Drivers', value: drivers?.totalCount ?? 0, icon: Car, color: 'text-gray-600 bg-gray-50' },
  ]

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Moderator Dashboard</h1>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {stats.map(({ label, value, icon: Icon, color }) => (
          <Card key={label}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className={`p-2 rounded-lg ${color}`}><Icon className="h-4 w-4" /></div>
                <div>
                  <p className="text-xl font-bold text-gray-900 dark:text-white">{value}</p>
                  <p className="text-xs text-gray-500">{label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <CardTitle className="text-base">Recent Reports</CardTitle>
            <Button asChild variant="ghost" size="sm"><Link to="/moderator/reports">View all</Link></Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {reports?.items.map((r) => (
                <div key={r.id} className="flex items-center justify-between border-b border-gray-100 dark:border-gray-800 pb-2 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{r.category.replace(/([A-Z])/g, ' $1').trim()}</p>
                    <p className="text-xs text-gray-500">{r.severity} severity</p>
                  </div>
                  <Badge variant={r.status === 'Pending' ? 'warning' : r.status === 'Approved' ? 'success' : 'secondary'}>
                    {r.status}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <CardTitle className="text-base">High-Risk Drivers</CardTitle>
            <Button asChild variant="ghost" size="sm"><Link to="/moderator/drivers">View all</Link></Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {drivers?.items.filter(d => d.status !== 'Safe').map((d) => (
                <div key={d.id} className="flex items-center justify-between border-b border-gray-100 dark:border-gray-800 pb-2 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{d.driverName}</p>
                    <p className="text-xs text-gray-500">{d.reportCount} reports</p>
                  </div>
                  <Badge variant={d.status === 'HighRisk' ? 'destructive' : 'warning'}>
                    {d.status === 'HighRisk' ? 'High Risk' : d.status}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
