import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { driverRightsApi } from '@/api/driverRights'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { Clock, ShieldCheck, ShieldX } from 'lucide-react'
import type { Appeal, AppealStatus } from '@/types'

const statusVariant: Record<AppealStatus, 'warning' | 'success' | 'secondary' | 'destructive'> = {
  Received: 'warning',
  UnderReview: 'warning',
  Upheld: 'success',
  Dismissed: 'secondary',
  Withdrawn: 'secondary',
}

function ResolvePanel({
  onResolve,
  isPending,
}: {
  onResolve: (body: {
    status: 'Upheld' | 'Dismissed'
    outcome: string
    reviewedRiskScore: boolean
    reviewedDriverResponse: boolean
    reviewedScoringLogic: boolean
  }) => void
  isPending: boolean
}) {
  const [outcome, setOutcome] = useState('')
  const [reviewedResponse, setReviewedResponse] = useState(false)
  const [reviewedScore, setReviewedScore] = useState(false)
  const [reviewedLogic, setReviewedLogic] = useState(false)

  // Clause 35.3 — the driver's submission must actually have been considered.
  const blocked = outcome.trim().length < 10 || !reviewedResponse || isPending

  const resolve = (status: 'Upheld' | 'Dismissed') =>
    onResolve({
      status,
      outcome: outcome.trim(),
      reviewedRiskScore: reviewedScore,
      reviewedDriverResponse: reviewedResponse,
      reviewedScoringLogic: reviewedLogic,
    })

  return (
    <div className="mt-4 space-y-3 rounded-md border border-gray-200 p-3">
      <Textarea
        rows={3}
        placeholder="Your decision and the reasons for it (required, at least 10 characters). This is sent to the driver and recorded against your account."
        value={outcome}
        onChange={(e) => setOutcome(e.target.value)}
      />

      <div className="space-y-1 text-xs text-gray-600">
        <label className="flex items-center gap-2">
          <input
            type="checkbox" checked={reviewedResponse}
            onChange={(e) => setReviewedResponse(e.target.checked)}
          />
          I read the driver&apos;s submission <span className="text-red-500">*</span>
        </label>
        <label className="flex items-center gap-2">
          <input
            type="checkbox" checked={reviewedScore}
            onChange={(e) => setReviewedScore(e.target.checked)}
          />
          I reviewed the risk score
        </label>
        <label className="flex items-center gap-2">
          <input
            type="checkbox" checked={reviewedLogic}
            onChange={(e) => setReviewedLogic(e.target.checked)}
          />
          I reviewed how the score was derived
        </label>
      </div>

      <div className="flex gap-2">
        <Button
          size="sm" variant="outline" disabled={blocked}
          className="text-green-600 border-green-300 hover:bg-green-50"
          onClick={() => resolve('Upheld')}
        >
          <ShieldCheck className="h-3 w-3 mr-1" />Uphold
        </Button>
        <Button
          size="sm" variant="outline" disabled={blocked}
          className="text-red-600 border-red-300 hover:bg-red-50"
          onClick={() => resolve('Dismissed')}
        >
          <ShieldX className="h-3 w-3 mr-1" />Dismiss
        </Button>
      </div>

      <p className="text-xs text-gray-500">
        Upholding resets the driver&apos;s public standing. Either outcome lifts the suspension,
        unless another appeal is still open.
      </p>
    </div>
  )
}

export function ModeratorAppealsPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['appeals'],
    queryFn: () => driverRightsApi.getAppeals({ pageSize: 50 }),
  })

  const resolve = useMutation({
    mutationFn: ({ id, body }: { id: string; body: Parameters<typeof driverRightsApi.resolveAppeal>[1] }) =>
      driverRightsApi.resolveAppeal(id, body),
    onSuccess: () => {
      toast.success('Appeal resolved')
      qc.invalidateQueries({ queryKey: ['appeals'] })
      qc.invalidateQueries({ queryKey: ['drivers'] })
    },
    onError: (error) => {
      const message =
        (error as { response?: { data?: { error?: string } } })?.response?.data?.error
        ?? 'Could not resolve that appeal.'
      toast.error(message)
    },
  })

  const overdue = (appeal: Appeal) =>
    !appeal.resolvedAt && new Date(appeal.dueAt) < new Date()

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Driver appeals</h1>

      {isLoading && <LoadingSpinner className="py-12" />}

      {data?.items.length === 0 && (
        <p className="text-sm text-gray-500">No appeals have been lodged.</p>
      )}

      <div className="space-y-3">
        {data?.items.map((appeal) => (
          <Card key={appeal.id}>
            <CardContent className="pt-4">
              <div className="flex flex-wrap items-center gap-2 mb-1">
                <span className="font-semibold text-gray-900 dark:text-white">
                  {appeal.driverName}
                </span>
                <Badge variant={statusVariant[appeal.status]}>{appeal.status}</Badge>
                <Badge variant="secondary">
                  {appeal.grounds.replace(/([A-Z])/g, ' $1').trim()}
                </Badge>
                {appeal.publicStatusSuspended && (
                  <Badge variant="warning">Public status suspended</Badge>
                )}
                {overdue(appeal) && (
                  <Badge variant="destructive" className="gap-1">
                    <Clock className="h-3 w-3" />Overdue
                  </Badge>
                )}
              </div>

              <p className="text-sm text-gray-500 mb-2">
                Lodged {new Date(appeal.createdAt).toLocaleDateString()} · Due{' '}
                {new Date(appeal.dueAt).toLocaleDateString()} · {appeal.contactEmail}
              </p>

              <p className="text-sm text-gray-700 dark:text-gray-300">{appeal.detail}</p>

              {appeal.outcome && (
                <p className="mt-2 rounded bg-gray-50 p-2 text-xs text-gray-600">
                  <span className="font-medium">Outcome: </span>{appeal.outcome}
                </p>
              )}

              {(appeal.status === 'Received' || appeal.status === 'UnderReview') && (
                <ResolvePanel
                  isPending={resolve.isPending}
                  onResolve={(body) => resolve.mutate({ id: appeal.id, body })}
                />
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
