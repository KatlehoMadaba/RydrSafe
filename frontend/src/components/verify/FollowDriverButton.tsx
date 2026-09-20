import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Bell, BellOff } from 'lucide-react'
import { toast } from 'sonner'
import { verificationApi } from '@/api/verification'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

export function FollowDriverButton({ driverId, className }: { driverId: string; className?: string }) {
  const qc = useQueryClient()

  const { data: isFollowing = false } = useQuery({
    queryKey: ['driver-follow', driverId],
    queryFn: () => verificationApi.getFollowStatus(driverId),
  })

  const followMutation = useMutation({
    mutationFn: () => (isFollowing ? verificationApi.unfollowDriver(driverId) : verificationApi.followDriver(driverId)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['driver-follow', driverId] })
      toast.success(
        isFollowing ? 'You will no longer receive alerts for this driver.' : 'You will be notified if this driver is flagged again.'
      )
    },
    onError: () => toast.error('Could not update follow status.'),
  })

  return (
    <Button
      variant={isFollowing ? 'secondary' : 'outline'}
      className={cn('w-full', className)}
      isLoading={followMutation.isPending}
      onClick={() => followMutation.mutate()}
    >
      {isFollowing ? (
        <>
          <BellOff className="h-4 w-4 mr-2" /> Unfollow Driver
        </>
      ) : (
        <>
          <Bell className="h-4 w-4 mr-2" /> Follow Driver — Get Alerts
        </>
      )}
    </Button>
  )
}
