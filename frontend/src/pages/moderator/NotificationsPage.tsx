import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationsApi } from '@/api/notifications'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { Bell, CheckCheck } from 'lucide-react'
import { cn } from '@/lib/utils'

export function ModeratorNotificationsPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['notifications'],
    queryFn: notificationsApi.getAll,
  })

  const markRead = useMutation({
    mutationFn: notificationsApi.markRead,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })

  const markAllRead = useMutation({
    mutationFn: notificationsApi.markAllRead,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })

  const unreadCount = data?.filter(n => !n.isRead).length ?? 0

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Notifications</h1>
          {unreadCount > 0 && <p className="text-sm text-gray-500">{unreadCount} unread</p>}
        </div>
        {unreadCount > 0 && (
          <Button variant="outline" size="sm" onClick={() => markAllRead.mutate()}>
            <CheckCheck className="h-4 w-4 mr-2" />Mark all read
          </Button>
        )}
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}

      {data?.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Bell className="h-10 w-10 text-gray-300 mx-auto mb-3" />
            <p className="text-gray-500">No notifications yet</p>
          </CardContent>
        </Card>
      )}

      <div className="space-y-2">
        {data?.map((n) => (
          <Card key={n.id} className={cn(!n.isRead && 'border-blue-200 bg-blue-50/50 dark:bg-blue-950/20')}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div className="flex-1">
                  <p className={cn('text-sm font-medium', n.isRead ? 'text-gray-700 dark:text-gray-300' : 'text-gray-900 dark:text-white')}>
                    {n.title}
                  </p>
                  <p className="text-sm text-gray-500 mt-0.5">{n.message}</p>
                  <p className="text-xs text-gray-400 mt-1">{new Date(n.createdAt).toLocaleString()}</p>
                </div>
                {!n.isRead && (
                  <button
                    className="shrink-0 text-xs text-blue-600 hover:underline"
                    onClick={() => markRead.mutate(n.id)}
                  >
                    Mark read
                  </button>
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
