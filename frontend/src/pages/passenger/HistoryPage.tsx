import { useQuery } from '@tanstack/react-query'
import { verificationApi } from '@/api/verification'
import { Card, CardContent } from '@/components/ui/card'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import { LoadingSpinner } from '@/components/LoadingSpinner'

export function HistoryPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['verification-history-all'],
    queryFn: () => verificationApi.getHistory({ pageSize: 50 }),
  })

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Verification History</h1>
        <p className="text-gray-500 mt-1">All drivers you've previously verified.</p>
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}

      {data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center text-gray-500">
            No verifications yet. Go verify your next driver!
          </CardContent>
        </Card>
      )}

      <div className="space-y-3">
        {data?.items.map((item) => (
          <Card key={item.id}>
            <CardContent className="pt-4 pb-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-gray-900 dark:text-white">{item.result.driverName || 'Unknown Driver'}</h3>
                    <RiskBadge status={item.result.status} />
                  </div>
                  <p className="text-sm text-gray-500">{item.result.registrationNumber}</p>
                  {item.result.vehicleMake && (
                    <p className="text-sm text-gray-500">{item.result.vehicleMake} {item.result.vehicleModel}</p>
                  )}
                  <p className="text-xs text-gray-400 mt-1">{new Date(item.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="w-40">
                  <RiskScore score={item.result.riskScore} />
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
