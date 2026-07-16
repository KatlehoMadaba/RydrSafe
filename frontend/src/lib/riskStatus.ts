import { ShieldCheck, Clock, ShieldAlert, OctagonAlert, SearchX, type LucideIcon } from 'lucide-react'
import type { DriverStatus } from '@/types'
import type { RingTone } from '@/components/ui/verification-ring'

/**
 * Single source of truth for risk presentation. Previously RiskBadge, RiskScore,
 * ModeratorDashboardPage and AnalyticsPage each carried their own status→colour
 * map, and disagreed (Flagged read as "destructive" in one and "warning" in
 * another). Every consumer now reads from here.
 */

export type RiskState = DriverStatus | 'NoCommunityRecord'

export interface RiskPresentation {
  /** Short label for badges and chips. */
  label: string
  /** The hero headline — what the user actually wants to know. */
  headline: string
  /** The one line the user acts on. */
  recommendation: string
  /** An optional second line — used sparingly, only where a concrete next step exists. */
  secondary?: string
  icon: LucideIcon
  tone: RingTone
  /** rise-firm is reserved for the two states that genuinely warrant a firmer entrance. */
  entrance: 'animate-rise' | 'animate-rise-firm'
  /** Flagged/HighRisk escalate to role="alert"; everything else is role="status". */
  live: 'polite' | 'assertive'
}

export const RISK_PRESENTATION: Record<RiskState, RiskPresentation> = {
  Safe: {
    label: 'Safe',
    headline: 'Safe to ride',
    recommendation: 'No community reports on this driver. Ordinary care still applies.',
    icon: ShieldCheck,
    tone: 'safe',
    entrance: 'animate-rise',
    live: 'polite',
  },
  UnderReview: {
    label: 'Under Review',
    headline: 'Under review',
    recommendation: 'This driver has a report currently being reviewed by moderators. A little extra caution is reasonable.',
    icon: Clock,
    tone: 'review',
    entrance: 'animate-rise',
    live: 'polite',
  },
  Flagged: {
    label: 'Flagged',
    headline: 'Flagged by the community',
    recommendation: 'Multiple community reports have been made about this driver.',
    secondary: 'If another driver is available, we recommend requesting one.',
    icon: ShieldAlert,
    tone: 'flagged',
    entrance: 'animate-rise-firm',
    live: 'assertive',
  },
  HighRisk: {
    label: 'High Risk',
    headline: 'High risk',
    recommendation: 'We recommend you do not continue this trip. Cancel and request a different driver.',
    secondary: 'If you feel unsafe, contact local emergency services.',
    icon: OctagonAlert,
    tone: 'highrisk',
    entrance: 'animate-rise-firm',
    live: 'assertive',
  },
  NoCommunityRecord: {
    label: 'No Record',
    headline: 'No community record yet',
    recommendation:
      "Nobody has verified or reported this driver yet. That isn't the same as a clean record — there simply isn't enough community information. Ordinary care applies.",
    icon: SearchX,
    tone: 'norecord',
    entrance: 'animate-rise',
    live: 'polite',
  },
}

/** A driver record can still exist without a match on this specific lookup — matchFound, not riskScore, is what separates "clean" from "unknown". */
export function resolveRiskState(input: { status: DriverStatus; matchFound?: boolean }): RiskState {
  if (input.matchFound === false) return 'NoCommunityRecord'
  return input.status
}

// Literal per-tone classes (Tailwind v4 scans source text, so this can't be built by interpolation).
export const STATUS_ZONE_BG: Record<RingTone, string> = {
  safe: 'bg-safe-soft',
  review: 'bg-review-soft',
  flagged: 'bg-flagged-soft',
  highrisk: 'bg-highrisk-soft',
  norecord: 'bg-norecord-soft',
}

/** Mirrors the backend thresholds in DriverStatusPolicy.cs (>=80 HighRisk, >=60 Flagged, >=30 UnderReview). */
export function scoreTone(score: number): RingTone {
  if (score < 30) return 'safe'
  if (score < 60) return 'review'
  if (score < 80) return 'flagged'
  return 'highrisk'
}
