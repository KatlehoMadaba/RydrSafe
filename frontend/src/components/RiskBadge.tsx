import { Badge } from '@/components/ui/badge'
import { RISK_PRESENTATION, type RiskState } from '@/lib/riskStatus'

export function RiskBadge({ state }: { state: RiskState }) {
  const { label, icon, tone } = RISK_PRESENTATION[state]
  return (
    <Badge variant={tone} icon={icon}>
      {label}
    </Badge>
  )
}
