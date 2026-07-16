import { VerificationRing } from '@/components/ui/verification-ring'

// The backend does OCR, matching and scoring in one call — there's no real
// per-step progress to report. This ticker is honest about that: it names what's
// happening, not a false sense of "step 2 of 3 complete".
const STEPS = ['Submitting to RydrSafe', 'Matching driver record', 'Checking community reports']

export function VerificationScanning() {
  return (
    <div className="flex flex-col items-center gap-6 rounded-2xl border border-border bg-card px-6 py-12 text-center shadow-md animate-fade-in">
      <VerificationRing tone="norecord" size="lg" variant="scanning" />
      <div className="space-y-1">
        <p className="font-display text-xl font-bold">Checking community reports…</p>
        <p className="text-sm text-muted-foreground">This usually takes a few seconds.</p>
      </div>
      <ul className="w-full max-w-xs space-y-2 text-left text-sm text-muted-foreground">
        {STEPS.map((step) => (
          <li key={step} className="flex items-center gap-2">
            <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-teal-500" />
            {step}
          </li>
        ))}
      </ul>
    </div>
  )
}
