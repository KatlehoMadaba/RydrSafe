import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Search, Flag, History, ShieldCheck, AlertTriangle, TrendingUp, Users, ChevronRight } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { StatCard, StatCardGrid } from '@/components/dashboard/StatCard'
import { useAuth } from '@/hooks/useAuth'
import { verificationApi } from '@/api/verification'
import { reportsApi } from '@/api/reports'
import { followApi } from '@/api/follow'
import { RiskBadge } from '@/components/RiskBadge'
import { Badge } from '@/components/ui/badge'

/** Matches the moderator queue, so one severity reads the same colour everywhere. */
const severityVariant: Record<string, 'success' | 'warning' | 'destructive'> = {
  Low: 'success',
  Medium: 'warning',
  High: 'destructive',
  Critical: 'destructive',
}

/**
 * Clause 6.2 in words a reporter can act on. "Pending" and "Corroborated" are our internal
 * pipeline names; what the person who filed it wants to know is whether anything has happened
 * to it yet and whether anyone else can see it.
 */
const reportStatusLabel: Record<string, string> = {
  Pending: 'Reported — awaiting moderator review',
  Approved: 'Reviewed — not yet shown to others',
  Corroborated: 'Confirmed — visible to other users',
  Rejected: 'Reviewed — not upheld',
  Withdrawn: 'Withdrawn by you',
}

export function PassengerDashboardPage() {
  const { user } = useAuth()

  const { data: recentHistory } = useQuery({
    queryKey: ['verification-history-recent'],
    queryFn: () => verificationApi.getHistory({ pageSize: 5 }),
  })

  const { data: myReports } = useQuery({
    queryKey: ['my-reports-recent'],
    queryFn: () => reportsApi.getMine(),
  })

  const { data: followedDrivers } = useQuery({
    queryKey: ['followed-drivers'],
    queryFn: () => followApi.getFollowedDrivers(),
  })

  const { data: verificationStats, isLoading: statsLoading } = useQuery({
    queryKey: ['verification-stats'],
    queryFn: () => verificationApi.getStats(),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-foreground">Welcome back, {user?.fullName?.split(' ')[0]}</h1>
        <p className="text-muted-foreground mt-1">Stay safe — verify your driver before every ride.</p>
      </div>

      <StatCardGrid columns={3}>
        <StatCard label="Verifications Done" value={verificationStats?.total ?? 0} icon={ShieldCheck} accent="navy" isLoading={statsLoading} />
        <StatCard
          label="Flagged Drivers Found"
          value={verificationStats?.flagged ?? 0}
          icon={AlertTriangle}
          accent="highrisk"
          isLoading={statsLoading}
        />
        <StatCard label="Safe Verifications" value={verificationStats?.safe ?? 0} icon={TrendingUp} accent="safe" isLoading={statsLoading} />
      </StatCardGrid>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card className="border-2 border-teal-100 hover:border-teal-300 transition-colors">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center gap-3 py-4">
              <div className="p-4 bg-teal-50 rounded-full">
                <Search className="h-8 w-8 text-teal-600" />
              </div>
              <h3 className="font-semibold text-foreground">Verify a Driver</h3>
              <p className="text-sm text-muted-foreground">Upload your ride screenshot to check a driver's safety history</p>
              <Button asChild className="mt-2">
                <Link to="/passenger/verify">Verify Now</Link>
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card className="border-2 border-review-muted hover:border-review transition-colors">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center gap-3 py-4">
              <div className="p-4 bg-review-soft rounded-full">
                <Flag className="h-8 w-8 text-review" />
              </div>
              <h3 className="font-semibold text-foreground">Report a Driver</h3>
              <p className="text-sm text-muted-foreground">Help the community by reporting unsafe or suspicious behaviour</p>
              <Button asChild variant="soft" className="mt-2">
                <Link to="/passenger/report">File Report</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      {followedDrivers && followedDrivers.length > 0 && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-highrisk" />
              Flagged Drivers You're Tracking
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {followedDrivers.map((d) => (
                <div key={d.id} className="flex items-center justify-between border-b border-border pb-2 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-foreground">{d.driverName}</p>
                    <p className="text-xs text-muted-foreground">
                      {d.registrationNumber ?? '—'} · {d.reportCount} report{d.reportCount !== 1 ? 's' : ''}
                    </p>
                  </div>
                  <RiskBadge state={d.status} />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <Link to="/passenger/community-reports" className="flex items-center gap-3 p-4">
          <Users className="h-4 w-4 text-subtle" />
          <div className="min-w-0 flex-1">
            <span className="block text-sm font-medium text-foreground">Community reports</span>
            <span className="block text-xs text-muted-foreground">
              See what other passengers have confirmed about a driver
            </span>
          </div>
          <ChevronRight className="h-4 w-4 shrink-0 text-subtle" />
        </Link>
      </Card>

      {/* Side by side: these are the two things a passenger has actually done, and reading them
          together is what shows whether a driver they checked is one they went on to report.
          Each collapses to full width on a phone. */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 items-start">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Recent Verifications</CardTitle>
            <Button asChild variant="ghost" size="sm">
              <Link to="/passenger/history">
                View all <History className="ml-1 h-3 w-3" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent>
            {recentHistory && recentHistory.items.length > 0 ? (
              <div className="space-y-3">
                {recentHistory.items.slice(0, 5).map((item) => (
                  <div key={item.id} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                    <div className="min-w-0">
                      <p className="font-medium text-sm text-foreground truncate">{item.driverName}</p>
                      <p className="text-xs text-muted-foreground truncate">{item.registrationNumber}</p>
                    </div>
                    <RiskBadge state={item.status} />
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">
                You have not verified a driver yet.
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Your Reports</CardTitle>
            <Button asChild variant="ghost" size="sm">
              <Link to="/passenger/report">
                Report a driver <Flag className="ml-1 h-3 w-3" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent>
            {myReports && myReports.length > 0 ? (
              <div className="space-y-3">
                {myReports.slice(0, 5).map((report) => (
                  <div key={report.id} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                    <div className="min-w-0">
                      <p className="font-medium text-sm text-foreground truncate">{report.driverName}</p>
                      <p className="text-xs text-muted-foreground truncate">
                        {reportStatusLabel[report.status] ?? report.status}
                        {report.isAnonymous && ' · anonymous'}
                      </p>
                    </div>
                    <Badge variant={severityVariant[report.severity] ?? 'secondary'}>
                      {report.severity}
                    </Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">
                You have not reported a driver yet.
              </p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
