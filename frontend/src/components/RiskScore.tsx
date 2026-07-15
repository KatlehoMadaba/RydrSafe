import { Progress } from '@/components/ui/progress'
import { scoreTone } from '@/lib/riskStatus'
import { cn } from '@/lib/utils'

// Literal per-tone classes — scoreTone() returns the tone name, not a class string,
// so Tailwind's static scanner can still see every class used here.
const TEXT_CLASS = {
  safe: 'text-safe-strong',
  review: 'text-review-strong',
  flagged: 'text-flagged-strong',
  highrisk: 'text-highrisk-strong',
  norecord: 'text-norecord-strong',
} as const

const BAR_CLASS = {
  safe: 'bg-safe',
  review: 'bg-review',
  flagged: 'bg-flagged',
  highrisk: 'bg-highrisk',
  norecord: 'bg-norecord',
} as const

export function RiskScore({ score }: { score: number }) {
  const tone = scoreTone(score)

  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between">
        <span className="text-xs text-muted-foreground">Risk Score</span>
        <span className={cn('text-sm font-bold', TEXT_CLASS[tone])}>{score}/100</span>
      </div>
      <Progress value={score} indicatorClassName={BAR_CLASS[tone]} aria-label={`Risk score ${score} out of 100`} />
    </div>
  )
}
