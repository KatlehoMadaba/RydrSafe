import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { VerificationRing } from '@/components/ui/verification-ring'
import { CommunityEvidence } from './CommunityEvidence'
import { resolveRiskState, RISK_PRESENTATION, STATUS_ZONE_BG } from '@/lib/riskStatus'
import type { VerificationResult } from '@/types'
import { cn } from '@/lib/utils'

/**
 * The hero of the app. A focused answer, not a dashboard — hierarchy is
 * Status → Recommendation → Score → Evidence, in that order, because a user
 * standing at a car door wants the answer first, the action second, and the
 * number last.
 */
export function VerificationResultPanel({ result, className }: { result: VerificationResult; className?: string }) {
  const state = resolveRiskState(result)
  const presentation = RISK_PRESENTATION[state]
  const Icon = presentation.icon
  const isNoRecord = state === 'NoCommunityRecord'

  return (
    <div
      role={presentation.live === 'assertive' ? 'alert' : 'status'}
      aria-live={presentation.live}
      className={cn('overflow-hidden rounded-2xl border border-border bg-card shadow-md', presentation.entrance, className)}
    >
      {/* 1. Status */}
      <div className={cn('flex flex-col items-center gap-3 px-6 py-10 text-center', STATUS_ZONE_BG[presentation.tone])}>
        <VerificationRing
          tone={presentation.tone}
          icon={presentation.icon}
          score={result.riskScore}
          size="lg"
          variant={isNoRecord ? 'pending' : 'score'}
        />
        <h2 className="font-display text-3xl font-extrabold tracking-tight">{presentation.headline}</h2>
      </div>

      <div className="space-y-6 p-6">
        {/* 2. Recommendation */}
        <Alert variant={presentation.tone}>
          <Icon />
          <AlertTitle>{presentation.label}</AlertTitle>
          <AlertDescription>
            <p>{presentation.recommendation}</p>
            {presentation.secondary && <p>{presentation.secondary}</p>}
          </AlertDescription>
        </Alert>

        {/* 3. Risk score — text only. The ring already encodes it visually;
            re-drawing it as a bar here would just be noise. No record has no
            meaningful score to show. */}
        {!isNoRecord && (
          <p className="text-sm text-foreground">
            Risk score <span className="font-bold">{result.riskScore}</span> of 100
          </p>
        )}

        {/* 4. Supporting evidence */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 border-t border-border pt-4 text-sm">
          <div>
            <p className="text-xs text-muted-foreground">Driver name</p>
            <p className="font-semibold text-foreground">{result.driverName || '—'}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Registration</p>
            <p className="font-semibold text-foreground">{result.registrationNumber || '—'}</p>
          </div>
          {result.vehicleMake && (
            <div>
              <p className="text-xs text-muted-foreground">Vehicle</p>
              <p className="font-semibold text-foreground">
                {result.vehicleMake} {result.vehicleModel}
              </p>
            </div>
          )}
          {result.phoneNumber && (
            <div>
              <p className="text-xs text-muted-foreground">Phone</p>
              <p className="font-semibold text-foreground">{result.phoneNumber}</p>
            </div>
          )}
        </div>

        <CommunityEvidence reportCount={result.reportCount} />
      </div>
    </div>
  )
}
