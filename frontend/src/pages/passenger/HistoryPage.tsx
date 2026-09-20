import { useQuery } from '@tanstack/react-query'
import { verificationApi } from '@/api/verification'
import { Card, CardContent } from '@/components/ui/card'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ErrorState } from '@/components/ErrorState'

export function HistoryPage() {
  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['verification-history-all'],
    queryFn: () => verificationApi.getHistory({ pageSize: 50 }),
  })

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-foreground">Verification History</h1>
        <p className="text-muted-foreground mt-1">All drivers you've previously verified.</p>
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}
      {isError && <ErrorState message="Couldn't load your history." onRetry={() => refetch()} />}

      {!isLoading && !isError && data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center text-muted-foreground">No verifications yet. Go verify your next driver!</CardContent>
        </Card>
      )}

      <div className="space-y-3">
        {data?.items.map((item) => (
          <Card key={item.id}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-foreground">{item.driverName || 'Unknown Driver'}</h3>
                    <RiskBadge state={item.status} />
                  </div>
                  <p className="text-sm text-muted-foreground">{item.registrationNumber}</p>
                  <p className="text-xs text-subtle mt-1">{new Date(item.verifiedAt).toLocaleDateString()}</p>
                </div>
                <div className="w-40">
                  <RiskScore score={item.riskScore} />
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
