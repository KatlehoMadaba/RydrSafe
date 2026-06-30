import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Search, Flag, History, ShieldCheck, AlertTriangle, TrendingUp } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { verificationApi } from '@/api/verification'
import { RiskBadge } from '@/components/RiskBadge'

export function PassengerDashboardPage() {
  const { user } = useAuth()

  const { data: recentHistory } = useQuery({
    queryKey: ['verification-history-recent'],
    queryFn: () => verificationApi.getHistory({ pageSize: 5 }),
  })

  const { data: verificationStats } = useQuery({
    queryKey: ['verification-stats'],
    queryFn: () => verificationApi.getStats(),
  })

  const stats = [
    { label: 'Verifications Done', value: verificationStats?.total ?? 0, icon: ShieldCheck, color: 'text-blue-600 bg-blue-50' },
    { label: 'Flagged Drivers Found', value: verificationStats?.flagged ?? 0, icon: AlertTriangle, color: 'text-red-600 bg-red-50' },
    { label: 'Safe Verifications', value: verificationStats?.safe ?? 0, icon: TrendingUp, color: 'text-green-600 bg-green-50' },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
          Welcome back, {user?.fullName?.split(' ')[0]}
        </h1>
        <p className="text-gray-500 mt-1">Stay safe — verify your driver before every ride.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {stats.map(({ label, value, icon: Icon, color }) => (
          <Card key={label}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className={`p-3 rounded-lg ${color}`}>
                  <Icon className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
                  <p className="text-sm text-gray-500">{label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card className="border-2 border-blue-100 hover:border-blue-300 transition-colors">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center gap-3 py-4">
              <div className="p-4 bg-blue-50 rounded-full">
                <Search className="h-8 w-8 text-blue-600" />
              </div>
              <h3 className="font-semibold text-gray-900 dark:text-white">Verify a Driver</h3>
              <p className="text-sm text-gray-500">Upload your ride screenshot to check a driver's safety history</p>
              <Button asChild className="mt-2">
                <Link to="/passenger/verify">Verify Now</Link>
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card className="border-2 border-orange-100 hover:border-orange-300 transition-colors">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center gap-3 py-4">
              <div className="p-4 bg-orange-50 rounded-full">
                <Flag className="h-8 w-8 text-orange-600" />
              </div>
              <h3 className="font-semibold text-gray-900 dark:text-white">Report a Driver</h3>
              <p className="text-sm text-gray-500">Help the community by reporting unsafe or suspicious behaviour</p>
              <Button asChild variant="outline" className="mt-2 border-orange-300 text-orange-700 hover:bg-orange-50">
                <Link to="/passenger/report">File Report</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      {recentHistory && recentHistory.items.length > 0 && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Recent Verifications</CardTitle>
            <Button asChild variant="ghost" size="sm">
              <Link to="/passenger/history">View all <History className="ml-1 h-3 w-3" /></Link>
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {recentHistory.items.map((item) => (
                <div key={item.id} className="flex items-center justify-between py-2 border-b border-gray-100 dark:border-gray-800 last:border-0">
                  <div>
                    <p className="font-medium text-sm text-gray-900 dark:text-white">{item.driverName}</p>
                    <p className="text-xs text-gray-500">{item.registrationNumber}</p>
                  </div>
                  <RiskBadge status={item.status} />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
