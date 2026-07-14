import * as React from 'react'
import type { LucideIcon } from 'lucide-react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const badgeVariants = cva(
  'inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors',
  {
    variants: {
      variant: {
        // Legacy variants, retargeted onto tokens. Callsites migrate to the status
        // variants below in the risk-status pass, after which these can go.
        default: 'border-transparent bg-navy-100 text-navy-800',
        secondary: 'border-transparent bg-secondary text-secondary-foreground',
        destructive: 'border-transparent bg-highrisk-soft text-highrisk-strong',
        success: 'border-transparent bg-safe-soft text-safe-strong',
        warning: 'border-transparent bg-review-soft text-review-strong',
        outline: 'border-border text-foreground',

        // Status variants. These use the soft ramps rather than solid fills:
        // white on solid amber is only 4.7:1, which fails at badge text sizes.
        safe: 'border-safe-muted bg-safe-soft text-safe-strong',
        review: 'border-review-muted bg-review-soft text-review-strong',
        flagged: 'border-flagged-muted bg-flagged-soft text-flagged-strong',
        highrisk: 'border-highrisk-muted bg-highrisk-soft text-highrisk-strong',
        norecord: 'border-norecord-muted bg-norecord-soft text-norecord-strong',
      },
    },
    defaultVariants: { variant: 'default' },
  }
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {
  /** Pairs an icon with the label so status never rests on colour alone. */
  icon?: LucideIcon
}

function Badge({ className, variant, icon: Icon, children, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant }), className)} {...props}>
      {Icon ? <Icon className="h-3.5 w-3.5 shrink-0" aria-hidden="true" /> : null}
      {children}
    </div>
  )
}

export { Badge, badgeVariants }
