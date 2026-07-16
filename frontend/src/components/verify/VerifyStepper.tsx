import { Check } from 'lucide-react'
import { cn } from '@/lib/utils'

const STEPS = ['Upload', 'Review & confirm', 'Result']

export function VerifyStepper({ step }: { step: 1 | 2 | 3 }) {
  return (
    <ol className="flex items-center gap-2" aria-label="Verification progress">
      {STEPS.map((label, i) => {
        const n = i + 1
        const active = n === step
        const done = n < step
        return (
          <li key={label} className="flex flex-1 items-center gap-2 last:flex-none">
            <span
              className={cn(
                'flex h-6 w-6 shrink-0 items-center justify-center rounded-full border text-[11px] font-semibold',
                active && 'border-teal-500 bg-teal-500 text-white',
                done && 'border-teal-500 text-teal-600',
                !active && !done && 'border-border text-subtle'
              )}
              aria-current={active ? 'step' : undefined}
            >
              {done ? <Check className="h-3.5 w-3.5" /> : n}
            </span>
            <span className={cn('text-xs font-medium', active ? 'text-foreground' : 'text-muted-foreground')}>{label}</span>
            {n < STEPS.length && <span className="h-px flex-1 bg-border" aria-hidden="true" />}
          </li>
        )
      })}
    </ol>
  )
}
