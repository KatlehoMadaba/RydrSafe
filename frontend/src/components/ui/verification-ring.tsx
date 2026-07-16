import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * The circular verification indicator — RydrSafe's recurring visual motif.
 * Shield-inspired geometry without a literal shield, sized for everything from
 * an inline list row to the verification hero.
 *
 * Purely decorative: it is aria-hidden, and callers must always render the score
 * and status as real text alongside it.
 */

export type RingTone = 'safe' | 'review' | 'flagged' | 'highrisk' | 'norecord'
export type RingSize = 'xs' | 'sm' | 'md' | 'lg'

// `xs` carries no icon: at 20px the glyph collapses into noise and reads worse
// than the bare arc. In list rows it sits beside a text label anyway.
const SIZES: Record<RingSize, { px: number; stroke: number; icon: string | null }> = {
  xs: { px: 20, stroke: 3, icon: null },
  sm: { px: 40, stroke: 3.5, icon: 'h-4 w-4' },
  md: { px: 64, stroke: 5, icon: 'h-6 w-6' },
  lg: { px: 128, stroke: 8, icon: 'h-8 w-8' },
}

const TONES: Record<RingTone, { stroke: string; icon: string; track: string }> = {
  safe: { stroke: 'stroke-safe', icon: 'text-safe', track: 'stroke-safe-muted' },
  review: { stroke: 'stroke-review', icon: 'text-review', track: 'stroke-review-muted' },
  flagged: { stroke: 'stroke-flagged', icon: 'text-flagged', track: 'stroke-flagged-muted' },
  highrisk: { stroke: 'stroke-highrisk', icon: 'text-highrisk', track: 'stroke-highrisk-muted' },
  norecord: { stroke: 'stroke-norecord', icon: 'text-norecord', track: 'stroke-norecord-muted' },
}

export interface VerificationRingProps {
  /** 0–100. Ignored when `variant` is not `score`. */
  score?: number
  tone: RingTone
  size?: RingSize
  icon?: LucideIcon
  /**
   * `score` fills to the score. `pending` draws a dashed track for the
   * no-community-record state — an unknown driver has no score to show, and a
   * ring at zero would read as a clean result. `scanning` sweeps while work is
   * in flight.
   */
  variant?: 'score' | 'pending' | 'scanning'
  className?: string
}

export function VerificationRing({
  score = 0,
  tone,
  size = 'md',
  icon: Icon,
  variant = 'score',
  className,
}: VerificationRingProps) {
  const { px, stroke, icon: iconSize } = SIZES[size]
  const toneStyles = TONES[tone]

  const radius = (px - stroke) / 2
  const circumference = 2 * Math.PI * radius
  const clamped = Math.min(100, Math.max(0, score))
  const offsetTarget = circumference * (1 - clamped / 100)

  return (
    <div
      className={cn('relative inline-flex shrink-0 items-center justify-center', className)}
      style={{ width: px, height: px }}
      aria-hidden="true"
    >
      <svg width={px} height={px} viewBox={`0 0 ${px} ${px}`} className="-rotate-90">
        {/* Track */}
        <circle
          cx={px / 2}
          cy={px / 2}
          r={radius}
          fill="none"
          strokeWidth={stroke}
          strokeLinecap="round"
          className={cn(variant === 'pending' ? toneStyles.track : 'stroke-secondary')}
          strokeDasharray={variant === 'pending' ? `${stroke * 1.5} ${stroke * 2}` : undefined}
        />

        {/* Fill. Pending has no meaningful score, so it draws no arc at all. */}
        {variant !== 'pending' && (
          <circle
            cx={px / 2}
            cy={px / 2}
            r={radius}
            fill="none"
            strokeWidth={stroke}
            strokeLinecap="round"
            className={cn(
              toneStyles.stroke,
              variant === 'scanning' ? 'origin-center animate-scan' : 'animate-ring-draw'
            )}
            strokeDasharray={variant === 'scanning' ? `${circumference * 0.25} ${circumference}` : circumference}
            style={
              variant === 'scanning'
                ? undefined
                : ({
                    '--ring-circumference': circumference,
                    '--ring-offset-target': offsetTarget,
                    strokeDashoffset: offsetTarget,
                  } as React.CSSProperties)
            }
          />
        )}
      </svg>

      {Icon && iconSize ? (
        <span className="absolute inset-0 flex items-center justify-center">
          <Icon className={cn(iconSize, toneStyles.icon)} />
        </span>
      ) : null}
    </div>
  )
}
