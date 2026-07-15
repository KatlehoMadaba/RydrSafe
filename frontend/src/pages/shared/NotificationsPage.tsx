import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationsApi } from '@/api/notifications'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { Bell, CheckCheck } from 'lucide-react'
import { cn } from '@/lib/utils'

// Role-agnostic — it only ever calls notificationsApi, so the same page serves
// /passenger/alerts and /moderator/notifications.
export function NotificationsPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['notifications'],
    queryFn: notificationsApi.getAll,
  })

  const markRead = useMutation({
    mutationFn: notificationsApi.markRead,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })

  const unread = data?.filter((n) => !n.isRead) ?? []

  // The API only exposes PUT /api/notifications/{id}/read — there's no
  // read-all endpoint, so "mark all" fans out one request per unread item.
  const markAllRead = useMutation({
    mutationFn: () => Promise.all(unread.map((n) => notificationsApi.markRead(n.id))),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold text-foreground">Alerts</h1>
          {unread.length > 0 && <p className="text-sm text-muted-foreground">{unread.length} unread</p>}
        </div>
        {unread.length > 0 && (
          <Button variant="outline" size="sm" isLoading={markAllRead.isPending} onClick={() => markAllRead.mutate()}>
            <CheckCheck className="h-4 w-4 mr-2" />
            Mark all read
          </Button>
        )}
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}

      {data?.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Bell className="h-10 w-10 text-subtle mx-auto mb-3" />
            <p className="text-muted-foreground">No alerts yet</p>
          </CardContent>
        </Card>
      )}

      <div className="space-y-2">
        {data?.map((n) => (
          <Card key={n.id} className={cn(!n.isRead && 'border-teal-200 bg-teal-50/60')}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div className="flex-1">
                  <p className={cn('text-sm font-medium', n.isRead ? 'text-muted-foreground' : 'text-foreground')}>{n.title}</p>
                  <p className="text-sm text-muted-foreground mt-0.5">{n.message}</p>
                  <p className="text-xs text-subtle mt-1">{new Date(n.createdAt).toLocaleString()}</p>
                </div>
                {!n.isRead && (
                  <button className="shrink-0 text-xs text-teal-600 hover:underline" onClick={() => markRead.mutate(n.id)}>
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
