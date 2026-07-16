import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { usersApi } from '@/api/users'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ErrorState } from '@/components/ErrorState'
import { UserX, UserCheck } from 'lucide-react'

export function AdminModeratorsPage() {
  const qc = useQueryClient()

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['moderators-list'],
    queryFn: () => usersApi.getAll({ role: 'moderator', pageSize: 50 }),
  })

  const remove = useMutation({
    mutationFn: (id: string) => usersApi.updateRole(id, 'passenger'),
    onSuccess: () => {
      toast.success('Moderator removed')
      qc.invalidateQueries({ queryKey: ['moderators-list'] })
    },
    onError: () => toast.error('Could not remove moderator. Please try again.'),
  })

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Moderators</h1>

      {isLoading && <LoadingSpinner className="py-12" />}
      {isError && <ErrorState message="Couldn't load moderators." onRetry={() => refetch()} />}

      {!isLoading && !isError && data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <UserCheck className="h-10 w-10 text-subtle mx-auto mb-3" />
            <p className="text-muted-foreground">No moderators yet. Promote a user from the Users page.</p>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {data?.items.map((mod) => (
          <Card key={mod.id}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="h-10 w-10 rounded-full bg-primary flex items-center justify-center text-primary-foreground font-semibold">
                    {mod.fullName[0]?.toUpperCase()}
                  </div>
                  <div>
                    <p className="font-medium text-foreground">{mod.fullName}</p>
                    <p className="text-sm text-muted-foreground">{mod.email}</p>
                    <p className="text-xs text-subtle">Since {new Date(mod.createdAt).toLocaleDateString()}</p>
                  </div>
                </div>
                <Button
                  size="sm"
                  variant="ghost"
                  className="text-highrisk hover:text-highrisk-strong hover:bg-highrisk-soft"
                  isLoading={remove.isPending && remove.variables === mod.id}
                  onClick={() => remove.mutate(mod.id)}
                >
                  <UserX className="h-4 w-4 mr-1" />
                  Remove
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
