import { useQuery } from '@tanstack/react-query'
import { Users, UserCheck, FileText, Car } from 'lucide-react'
import { StatCard, StatCardGrid } from '@/components/dashboard/StatCard'
import { usersApi } from '@/api/users'
import { reportsApi } from '@/api/reports'
import { driversApi } from '@/api/drivers'

export function AdminDashboardPage() {
  const { data: users, isLoading: usersLoading } = useQuery({ queryKey: ['users'], queryFn: () => usersApi.getAll({ pageSize: 1 }) })
  const { data: moderators, isLoading: modsLoading } = useQuery({
    queryKey: ['moderators'],
    queryFn: () => usersApi.getAll({ role: 'moderator', pageSize: 1 }),
  })
  const { data: reports, isLoading: reportsLoading } = useQuery({
    queryKey: ['reports-admin'],
    queryFn: () => reportsApi.getAll({ pageSize: 1 }),
  })
  const { data: drivers, isLoading: driversLoading } = useQuery({
    queryKey: ['drivers-admin'],
    queryFn: () => driversApi.getAll({ pageSize: 1 }),
  })

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Admin Dashboard</h1>

      <StatCardGrid>
        <StatCard label="Total Users" value={users?.totalCount ?? 0} icon={Users} accent="navy" isLoading={usersLoading} />
        <StatCard label="Moderators" value={moderators?.totalCount ?? 0} icon={UserCheck} accent="teal" isLoading={modsLoading} />
        <StatCard label="Total Reports" value={reports?.totalCount ?? 0} icon={FileText} accent="review" isLoading={reportsLoading} />
        <StatCard label="Drivers in DB" value={drivers?.totalCount ?? 0} icon={Car} accent="neutral" isLoading={driversLoading} />
      </StatCardGrid>
    </div>
  )
}
