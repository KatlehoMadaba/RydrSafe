import { VerificationResultPanel } from '@/components/verify/VerificationResultPanel'
import { VerificationScanning } from '@/components/verify/VerificationScanning'
import type { VerificationResult } from '@/types'

/**
 * Every verification outcome, side by side. HighRisk and NoCommunityRecord are
 * near-impossible to reach by hand without seeding backend data — without this
 * page, the hero panel is unreviewable. DEV-only, gated in App.tsx.
 */

const BASE: Omit<VerificationResult, 'status' | 'riskScore' | 'reportCount' | 'matchFound'> = {
  driverName: 'Thabo Nkosi',
  registrationNumber: 'GP12ABGP',
  phoneNumber: '0821234567',
  driverId: 'demo-driver-id',
}

const RESULTS: { title: string; result: VerificationResult }[] = [
  { title: 'Safe', result: { ...BASE, status: 'Safe', riskScore: 12, reportCount: 0, matchFound: true } },
  { title: 'Under Review', result: { ...BASE, status: 'UnderReview', riskScore: 44, reportCount: 2, matchFound: true } },
  { title: 'Flagged', result: { ...BASE, status: 'Flagged', riskScore: 68, reportCount: 6, matchFound: true } },
  {
    title: 'High Risk',
    result: {
      ...BASE,
      vehicleMake: 'Toyota',
      vehicleModel: 'Corolla Quest',
      status: 'HighRisk',
      riskScore: 91,
      reportCount: 17,
      matchFound: true,
    },
  },
  {
    title: 'No community record',
    result: {
      driverName: 'Unknown Driver',
      registrationNumber: 'CA99ZZGP',
      status: 'Safe',
      riskScore: 0,
      reportCount: 0,
      matchFound: false,
    },
  },
]

export function VerificationStatesPage() {
  return (
    <div className="min-h-screen bg-background px-6 py-10">
      <div className="mx-auto max-w-6xl space-y-8">
        <div>
          <h1 className="font-display text-xl font-extrabold tracking-tight">Verification states</h1>
          <p className="text-xs text-muted-foreground">Calm Guardian — dev only. Status → Recommendation → Score → Evidence.</p>
        </div>

        <div className="grid gap-8 sm:grid-cols-2 xl:grid-cols-3">
          {RESULTS.map(({ title, result }) => (
            <div key={title} className="space-y-2">
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{title}</p>
              <VerificationResultPanel result={result} />
            </div>
          ))}
          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Scanning</p>
            <VerificationScanning />
          </div>
        </div>
      </div>
    </div>
  )
}
