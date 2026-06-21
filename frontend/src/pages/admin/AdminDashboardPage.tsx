import { useQuery } from '@tanstack/react-query'
import { Users, UserCheck, FileText, Car, TrendingUp } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { usersApi } from '@/api/users'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'

export function AdminDashboardPage() {
  const { data: users } = useQuery({ queryKey: ['users'], queryFn: () => usersApi.getAll({ pageSize: 1 }) })
  const { data: moderators } = useQuery({ queryKey: ['moderators'], queryFn: () => usersApi.getAll({ role: 'moderator', pageSize: 1 }) })
  const { data: reports } = useQuery({ queryKey: ['reports-admin'], queryFn: () => reportsApi.getAll({ pageSize: 1 }) })
  const { data: drivers } = useQuery({ queryKey: ['drivers-admin'], queryFn: () => driversApi.getAll({ pageSize: 1 }) })

  const stats = [
    { label: 'Total Users', value: users?.total ?? 0, icon: Users, color: 'text-blue-600 bg-blue-50' },
    { label: 'Moderators', value: moderators?.total ?? 0, icon: UserCheck, color: 'text-purple-600 bg-purple-50' },
    { label: 'Total Reports', value: reports?.total ?? 0, icon: FileText, color: 'text-orange-600 bg-orange-50' },
    { label: 'Drivers in DB', value: drivers?.total ?? 0, icon: Car, color: 'text-gray-600 bg-gray-50' },
  ]

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Admin Dashboard</h1>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {stats.map(({ label, value, icon: Icon, color }) => (
          <Card key={label}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className={`p-2 rounded-lg ${color}`}><Icon className="h-5 w-5" /></div>
                <div>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
                  <p className="text-xs text-gray-500">{label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <TrendingUp className="h-4 w-4" />System Health
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4 text-center">
            <div className="p-4 bg-green-50 dark:bg-green-950 rounded-lg">
              <p className="text-2xl font-bold text-green-700">99.9%</p>
              <p className="text-sm text-green-600">Uptime</p>
            </div>
            <div className="p-4 bg-blue-50 dark:bg-blue-950 rounded-lg">
              <p className="text-2xl font-bold text-blue-700">~240ms</p>
              <p className="text-sm text-blue-600">Avg Response</p>
            </div>
            <div className="p-4 bg-purple-50 dark:bg-purple-950 rounded-lg">
              <p className="text-2xl font-bold text-purple-700">~2.3s</p>
              <p className="text-sm text-purple-600">Avg OCR Time</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
