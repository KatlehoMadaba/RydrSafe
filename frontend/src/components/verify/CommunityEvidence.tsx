/**
 * Real facts only — never an invented trust or confidence score. Today the API
 * only returns `reportCount`, so this renders exactly one line, or nothing at
 * zero. Additional facts (last reported date, moderator review, follower count)
 * slot in here once the backend sends them — see plan Follow-ups.
 */
export function CommunityEvidence({ reportCount }: { reportCount: number }) {
  if (reportCount <= 0) return null

  return (
    <p className="text-sm text-muted-foreground">
      Based on {reportCount} community report{reportCount === 1 ? '' : 's'}.
    </p>
  )
}
