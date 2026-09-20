import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { FileText, Car, AlertTriangle, Clock } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { StatCard, StatCardGrid } from '@/components/dashboard/StatCard'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'
import { Badge } from '@/components/ui/badge'
import { RiskBadge } from '@/components/RiskBadge'

export function ModeratorDashboardPage() {
  const { data: reports, isLoading: reportsLoading } = useQuery({ queryKey: ['reports'], queryFn: () => reportsApi.getAll({ pageSize: 5 }) })
  const { data: drivers, isLoading: driversLoading } = useQuery({ queryKey: ['drivers'], queryFn: () => driversApi.getAll({ pageSize: 5 }) })
  const { data: flaggedCount, isLoading: flaggedLoading } = useQuery({
    queryKey: ['drivers', 'flagged-count'],
    queryFn: () => driversApi.getFlaggedCount(),
  })
  // Counted server-side rather than filtered out of the 5-item `reports` page above,
  // which would under-report the backlog the moment a 6th report is pending.
  const { data: pendingReports, isLoading: pendingLoading } = useQuery({
    queryKey: ['reports', 'Pending', 'count'],
    queryFn: () => reportsApi.getAll({ status: 'Pending', pageSize: 1 }),
  })

  const pending = pendingReports?.totalCount ?? 0

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Moderator Dashboard</h1>

      <StatCardGrid>
        <StatCard label="Pending Reports" value={pending} icon={Clock} accent="review" isLoading={pendingLoading} />
        <StatCard label="Total Reports" value={reports?.totalCount ?? 0} icon={FileText} accent="navy" isLoading={reportsLoading} />
        <StatCard label="Flagged Drivers" value={flaggedCount ?? 0} icon={AlertTriangle} accent="highrisk" isLoading={flaggedLoading} />
        <StatCard label="Total Drivers" value={drivers?.totalCount ?? 0} icon={Car} accent="neutral" isLoading={driversLoading} />
      </StatCardGrid>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <CardTitle className="text-base">Recent Reports</CardTitle>
            <Button asChild variant="ghost" size="sm">
              <Link to="/moderator/reports">View all</Link>
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {reports?.items.map((r) => (
                <div key={r.id} className="flex items-center justify-between border-b border-border pb-2 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-foreground">{r.category.replace(/([A-Z])/g, ' $1').trim()}</p>
                    <p className="text-xs text-muted-foreground">{r.severity} severity</p>
                  </div>
                  <Badge variant={r.status === 'Pending' ? 'warning' : r.status === 'Approved' ? 'success' : 'secondary'}>{r.status}</Badge>
                </div>
              ))}
              {reports?.items.length === 0 && <p className="text-sm text-muted-foreground text-center py-4">No reports yet.</p>}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <CardTitle className="text-base">High-Risk Drivers</CardTitle>
            <Button asChild variant="ghost" size="sm">
              <Link to="/moderator/drivers">View all</Link>
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {drivers?.items
                .filter((d) => d.status !== 'Safe')
                .map((d) => (
                  <div key={d.id} className="flex items-center justify-between border-b border-border pb-2 last:border-0">
                    <div>
                      <p className="text-sm font-medium text-foreground">{d.driverName}</p>
                      <p className="text-xs text-muted-foreground">{d.reportCount} reports</p>
                    </div>
                    <RiskBadge state={d.status} />
                  </div>
                ))}
              {drivers?.items.filter((d) => d.status !== 'Safe').length === 0 && (
                <p className="text-sm text-muted-foreground text-center py-4">No high-risk drivers right now.</p>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
