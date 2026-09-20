import type { LucideIcon } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

// The three dashboards (passenger/moderator/admin) previously copy-pasted this
// exact card shape with slightly different Tailwind palette colours each time.
// One component now carries all three, responds to density automatically, and
// shows a skeleton instead of a flashing "0" while data is in flight.

export type StatAccent = 'navy' | 'teal' | 'safe' | 'review' | 'flagged' | 'highrisk' | 'neutral'

const ACCENT_CLASSES: Record<StatAccent, { icon: string; chip: string }> = {
  navy: { icon: 'text-navy-700', chip: 'bg-navy-50' },
  teal: { icon: 'text-teal-600', chip: 'bg-teal-50' },
  safe: { icon: 'text-safe', chip: 'bg-safe-soft' },
  review: { icon: 'text-review', chip: 'bg-review-soft' },
  flagged: { icon: 'text-flagged', chip: 'bg-flagged-soft' },
  highrisk: { icon: 'text-highrisk', chip: 'bg-highrisk-soft' },
  neutral: { icon: 'text-muted-foreground', chip: 'bg-muted' },
}

export interface StatCardProps {
  label: string
  value: number | string
  icon: LucideIcon
  accent?: StatAccent
  isLoading?: boolean
}

export function StatCard({ label, value, icon: Icon, accent = 'neutral', isLoading }: StatCardProps) {
  const cls = ACCENT_CLASSES[accent]

  return (
    <Card>
      <CardContent className="pt-6 compact:pt-4">
        <div className="flex items-center gap-4 compact:gap-3">
          <div className={cn('shrink-0 rounded-lg p-3 compact:p-2', cls.chip)}>
            <Icon className={cn('h-5 w-5 compact:h-4 compact:w-4', cls.icon)} />
          </div>
          <div className="min-w-0">
            {isLoading ? (
              <Skeleton className="h-7 w-12 compact:h-6" />
            ) : (
              <p className="text-2xl compact:text-xl font-bold text-foreground">{value}</p>
            )}
            <p className="text-sm compact:text-xs text-muted-foreground">{label}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

export function StatCardGrid({ children, columns = 4 }: { children: React.ReactNode; columns?: 3 | 4 }) {
  // Icon + label needs real width to breathe — forcing 2 columns on a narrow
  // phone squeezes both into illegible slivers, so stack to 1 column below sm.
  return <div className={cn('grid grid-cols-1 gap-4 sm:grid-cols-2', columns === 4 ? 'md:grid-cols-4' : 'md:grid-cols-3')}>{children}</div>
}
