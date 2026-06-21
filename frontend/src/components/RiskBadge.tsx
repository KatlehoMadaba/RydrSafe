import { Badge } from '@/components/ui/badge'
import type { DriverStatus } from '@/types'

const config: Record<DriverStatus, { label: string; variant: 'success' | 'warning' | 'destructive' | 'default' }> = {
  Safe: { label: 'Safe', variant: 'success' },
  UnderReview: { label: 'Under Review', variant: 'warning' },
  Flagged: { label: 'Flagged', variant: 'destructive' },
  HighRisk: { label: 'High Risk', variant: 'destructive' },
}

export function RiskBadge({ status }: { status: DriverStatus }) {
  const { label, variant } = config[status]
  return <Badge variant={variant}>{label}</Badge>
}
