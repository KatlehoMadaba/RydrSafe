import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { usersApi } from '@/api/users'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { UserX, UserCheck } from 'lucide-react'

export function AdminModeratorsPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['moderators-list'],
    queryFn: () => usersApi.getAll({ role: 'moderator', pageSize: 50 }),
  })

  const remove = useMutation({
    mutationFn: (id: string) => usersApi.updateRole(id, 'passenger'),
    onSuccess: () => { toast.success('Moderator removed'); qc.invalidateQueries({ queryKey: ['moderators-list'] }) },
  })

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Moderators</h1>

      {isLoading && <LoadingSpinner className="py-12" />}

      {data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <UserCheck className="h-10 w-10 text-gray-300 mx-auto mb-3" />
            <p className="text-gray-500">No moderators yet. Promote a user from the Users page.</p>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {data?.items.map((mod) => (
          <Card key={mod.id}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="h-10 w-10 rounded-full bg-blue-600 flex items-center justify-center text-white font-semibold">
                    {mod.fullName[0]?.toUpperCase()}
                  </div>
                  <div>
                    <p className="font-medium text-gray-900 dark:text-white">{mod.fullName}</p>
                    <p className="text-sm text-gray-500">{mod.email}</p>
                    <p className="text-xs text-gray-400">Since {new Date(mod.createdAt).toLocaleDateString()}</p>
                  </div>
                </div>
                <Button size="sm" variant="ghost" className="text-red-500 hover:text-red-700 hover:bg-red-50"
                  onClick={() => remove.mutate(mod.id)}>
                  <UserX className="h-4 w-4 mr-1" />Remove
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
