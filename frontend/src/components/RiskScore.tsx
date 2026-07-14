import { Progress } from '@/components/ui/progress'
import { cn } from '@/lib/utils'

// Mirrors the backend thresholds in DriverStatusPolicy.cs (>=80 HighRisk,
// >=60 Flagged, >=30 UnderReview). The risk-status pass folds these into the
// shared source of truth alongside labels and icons.
function scoreTone(score: number) {
  if (score < 30) return { text: 'text-safe-strong', bar: 'bg-safe' }
  if (score < 60) return { text: 'text-review-strong', bar: 'bg-review' }
  if (score < 80) return { text: 'text-flagged-strong', bar: 'bg-flagged' }
  return { text: 'text-highrisk-strong', bar: 'bg-highrisk' }
}

export function RiskScore({ score }: { score: number }) {
  const tone = scoreTone(score)

  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between">
        <span className="text-xs text-muted-foreground">Risk Score</span>
        <span className={cn('text-sm font-bold', tone.text)}>{score}/100</span>
      </div>
      <Progress value={score} indicatorClassName={tone.bar} aria-label={`Risk score ${score} out of 100`} />
    </div>
  )
}
