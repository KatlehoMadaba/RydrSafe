import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'

import { cn } from '@/lib/utils'

const alertVariants = cva(
  "relative grid w-full grid-cols-[0_1fr] items-start gap-y-0.5 rounded-lg border px-4 py-3 text-sm has-[>svg]:grid-cols-[calc(var(--spacing)*4)_1fr] has-[>svg]:gap-x-3 [&>svg]:size-4 [&>svg]:translate-y-0.5 [&>svg]:text-current",
  {
    variants: {
      variant: {
        default: 'bg-card text-card-foreground',
        destructive:
          'bg-card text-destructive *:data-[slot=alert-description]:text-destructive/90 [&>svg]:text-current',

        // Status variants. The weighted left border is how a warning gets assertive
        // without getting loud — emphasis is structural, not chromatic.
        safe: 'border-safe-muted border-l-4 border-l-safe bg-safe-soft text-safe-strong *:data-[slot=alert-description]:text-safe-strong/90',
        review:
          'border-review-muted border-l-4 border-l-review bg-review-soft text-review-strong *:data-[slot=alert-description]:text-review-strong/90',
        flagged:
          'border-flagged-muted border-l-4 border-l-flagged bg-flagged-soft text-flagged-strong *:data-[slot=alert-description]:text-flagged-strong/90',
        highrisk:
          'border-highrisk-muted border-l-4 border-l-highrisk bg-highrisk-soft text-highrisk-strong *:data-[slot=alert-description]:text-highrisk-strong/90',
        norecord:
          'border-norecord-muted border-l-4 border-l-norecord bg-norecord-soft text-norecord-strong *:data-[slot=alert-description]:text-norecord-strong/90',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  }
)

function Alert({
  className,
  variant,
  // Defaults to the assertive `alert` live region, but callers can override it
  // (e.g. `role="presentation"`) when an outer element already owns the live
  // region and a nested one would double-announce.
  role = 'alert',
  ...props
}: React.ComponentProps<'div'> & VariantProps<typeof alertVariants>) {
  return <div data-slot="alert" role={role} className={cn(alertVariants({ variant }), className)} {...props} />
}

function AlertTitle({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      data-slot="alert-title"
      className={cn('col-start-2 line-clamp-1 min-h-4 font-medium tracking-tight', className)}
      {...props}
    />
  )
}

function AlertDescription({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      data-slot="alert-description"
      className={cn(
        'col-start-2 grid justify-items-start gap-1 text-sm text-muted-foreground [&_p]:leading-relaxed',
        className
      )}
      {...props}
    />
  )
}

export { Alert, AlertTitle, AlertDescription }
