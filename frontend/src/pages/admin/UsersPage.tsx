import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { usersApi } from '@/api/users'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ErrorState } from '@/components/ErrorState'
import { Search, UserX } from 'lucide-react'
import type { UserRole } from '@/types'

export function AdminUsersPage() {
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState('all')
  const qc = useQueryClient()

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['users', roleFilter],
    queryFn: () => usersApi.getAll({ role: roleFilter === 'all' ? undefined : roleFilter, pageSize: 50 }),
  })

  const updateRole = useMutation({
    mutationFn: ({ id, role }: { id: string; role: string }) => usersApi.updateRole(id, role),
    onSuccess: () => {
      toast.success('Role updated')
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: () => toast.error('Could not update role. Please try again.'),
  })

  const deactivate = useMutation({
    mutationFn: usersApi.deactivate,
    onSuccess: () => {
      toast.success('User deactivated')
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: () => toast.error('Could not deactivate user. Please try again.'),
  })

  const roleVariant: Record<UserRole, 'default' | 'secondary' | 'warning'> = {
    passenger: 'secondary',
    moderator: 'default',
    admin: 'warning',
  }

  const filtered =
    data?.items.filter(
      (u) => u.fullName.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase())
    ) ?? []

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Users</h1>

      <div className="flex gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-subtle" />
          <Input className="pl-9" placeholder="Search users…" value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
        <Select value={roleFilter} onValueChange={setRoleFilter}>
          <SelectTrigger className="w-36">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Roles</SelectItem>
            <SelectItem value="passenger">Passenger</SelectItem>
            <SelectItem value="moderator">Moderator</SelectItem>
            <SelectItem value="admin">Admin</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}
      {isError && <ErrorState message="Couldn't load users." onRetry={() => refetch()} />}

      {!isLoading && !isError && filtered.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Search className="h-10 w-10 text-subtle mx-auto mb-3" />
            <p className="text-muted-foreground">No users match your search.</p>
          </CardContent>
        </Card>
      )}

      <div className="space-y-2">
        {filtered.map((user) => (
          <Card key={user.id}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-center justify-between gap-4">
                <div className="flex items-center gap-3 min-w-0">
                  <div className="h-9 w-9 rounded-full bg-muted flex items-center justify-center text-sm font-semibold text-foreground shrink-0">
                    {user.fullName[0]?.toUpperCase()}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-foreground truncate">{user.fullName}</p>
                    <p className="text-xs text-muted-foreground truncate">{user.email}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 shrink-0">
                  <Badge variant={roleVariant[user.role] ?? 'secondary'} className="capitalize">
                    {user.role}
                  </Badge>
                  <Select value={user.role} onValueChange={(v) => updateRole.mutate({ id: user.id, role: v })}>
                    <SelectTrigger className="h-7 text-xs w-28">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="passenger">Passenger</SelectItem>
                      <SelectItem value="moderator">Moderator</SelectItem>
                      <SelectItem value="admin">Admin</SelectItem>
                    </SelectContent>
                  </Select>
                  <Button
                    size="sm"
                    variant="ghost"
                    aria-label={`Deactivate ${user.fullName}`}
                    className="text-highrisk hover:text-highrisk-strong hover:bg-highrisk-soft"
                    isLoading={deactivate.isPending && deactivate.variables === user.id}
                    onClick={() => deactivate.mutate(user.id)}
                  >
                    <UserX className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
