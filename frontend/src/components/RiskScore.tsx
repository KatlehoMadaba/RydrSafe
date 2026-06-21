import { Progress } from '@/components/ui/progress'
import { cn } from '@/lib/utils'

function scoreColor(score: number) {
  if (score < 30) return 'text-green-600'
  if (score < 60) return 'text-yellow-600'
  if (score < 80) return 'text-orange-600'
  return 'text-red-600'
}

function barColor(score: number) {
  if (score < 30) return '[&>div]:bg-green-500'
  if (score < 60) return '[&>div]:bg-yellow-500'
  if (score < 80) return '[&>div]:bg-orange-500'
  return '[&>div]:bg-red-500'
}

export function RiskScore({ score }: { score: number }) {
  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between">
        <span className="text-xs text-gray-500">Risk Score</span>
        <span className={cn('text-sm font-bold', scoreColor(score))}>{score}/100</span>
      </div>
      <Progress value={score} className={barColor(score)} />
    </div>
  )
}
