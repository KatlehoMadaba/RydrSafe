import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { reportsApi, type ModerationDecision } from '@/api/reports'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { CheckCircle, XCircle, ShieldAlert, Lock } from 'lucide-react'
import type { ReportStatus } from '@/types'

const severityVariant = {
  Low: 'success' as const,
  Medium: 'warning' as const,
  High: 'destructive' as const,
  Critical: 'destructive' as const,
}

const statusVariant: Record<ReportStatus, 'warning' | 'success' | 'destructive' | 'secondary'> = {
  Pending: 'warning',
  Approved: 'success',
  Corroborated: 'destructive',
  Rejected: 'secondary',
  Withdrawn: 'secondary',
}

/**
 * Clause 7.3(c) and POPIA s71(2): a decision needs a reason and a record of what was reviewed.
 * The API rejects anything less, so the form collects both rather than letting a moderator
 * click through and hit an error.
 */
function DecisionPanel({
  onDecide,
  isPending,
}: {
  onDecide: (decision: ModerationDecision, action: 'approve' | 'reject') => void
  isPending: boolean
}) {
  const [reason, setReason] = useState('')
  const [reviewedContent, setReviewedContent] = useState(false)
  const [reviewedResponse, setReviewedResponse] = useState(false)
  const [reviewedScore, setReviewedScore] = useState(false)

  const reasonTooShort = reason.trim().length < 10
  const blocked = reasonTooShort || !reviewedContent || isPending

  const decide = (action: 'approve' | 'reject') =>
    onDecide(
      {
        reason: reason.trim(),
        reviewedReportContent: reviewedContent,
        reviewedDriverResponse: reviewedResponse,
        reviewedRiskScore: reviewedScore,
      },
      action,
    )

  return (
    <div className="mt-4 space-y-3 rounded-md border border-gray-200 p-3">
      <Textarea
        placeholder="Reason for this decision (required, at least 10 characters). This is recorded against your account."
        value={reason}
        onChange={(e) => setReason(e.target.value)}
        rows={2}
      />

      <div className="space-y-1 text-xs text-gray-600">
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={reviewedContent}
            onChange={(e) => setReviewedContent(e.target.checked)}
          />
          I read the report and any supporting material <span className="text-red-500">*</span>
        </label>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={reviewedResponse}
            onChange={(e) => setReviewedResponse(e.target.checked)}
          />
          I considered the driver&apos;s response, where one exists
        </label>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={reviewedScore}
            onChange={(e) => setReviewedScore(e.target.checked)}
          />
          I reviewed the risk score and how it was derived
        </label>
      </div>

      <div className="flex gap-2">
        <Button
          size="sm" variant="outline" disabled={blocked}
          className="text-green-600 border-green-300 hover:bg-green-50"
          onClick={() => decide('approve')}
        >
          <CheckCircle className="h-3 w-3 mr-1" />Approve
        </Button>
        <Button
          size="sm" variant="outline" disabled={blocked}
          className="text-red-600 border-red-300 hover:bg-red-50"
          onClick={() => decide('reject')}
        >
          <XCircle className="h-3 w-3 mr-1" />Reject
        </Button>
      </div>

      <p className="text-xs text-gray-500">
        Approving does not publish this report. A Category A report only becomes visible to other
        users once the clause 6.3 corroboration threshold is met.
      </p>
    </div>
  )
}

export function ModeratorReportsPage() {
  const [statusFilter, setStatusFilter] = useState('all')
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['reports', statusFilter],
    queryFn: () =>
      reportsApi.getAll({
        status: statusFilter === 'all' ? undefined : statusFilter,
        pageSize: 50,
      }),
  })

  const decide = useMutation({
    mutationFn: ({
      id, decision, action,
    }: { id: string; decision: ModerationDecision; action: 'approve' | 'reject' }) =>
      action === 'approve'
        ? reportsApi.approve(id, decision)
        : reportsApi.reject(id, decision),
    onSuccess: (_result, variables) => {
      toast.success(variables.action === 'approve' ? 'Report approved' : 'Report rejected')
      qc.invalidateQueries({ queryKey: ['reports'] })
    },
    onError: (error) => {
      const message =
        (error as { response?: { data?: { error?: string } } })?.response?.data?.error
        ?? 'Could not record that decision.'
      toast.error(message)
    },
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Reports</h1>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-44"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All</SelectItem>
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="Approved">Approved (private)</SelectItem>
            <SelectItem value="Corroborated">Corroborated (public)</SelectItem>
            <SelectItem value="Rejected">Rejected</SelectItem>
            <SelectItem value="Withdrawn">Withdrawn</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}

      <div className="space-y-3">
        {data?.items.map((report) => (
          <Card key={report.id}>
            <CardContent className="pt-4">
              <div className="flex flex-wrap items-center gap-2 mb-1">
                <span className="font-semibold text-gray-900 dark:text-white">
                  {report.category.replace(/([A-Z])/g, ' $1').trim()}
                </span>
                <Badge variant={severityVariant[report.severity]}>{report.severity}</Badge>
                <Badge variant={statusVariant[report.status]}>{report.status}</Badge>

                {report.classification === 'CategoryA' && (
                  <Badge variant="destructive" className="gap-1">
                    <ShieldAlert className="h-3 w-3" />Category A
                  </Badge>
                )}
                {report.status === 'Approved' && (
                  <Badge variant="secondary" className="gap-1">
                    <Lock className="h-3 w-3" />Not public
                  </Badge>
                )}
                {report.reportedToPolice && <Badge variant="destructive">Police-reported</Badge>}
              </div>

              <p className="text-sm text-gray-500 mb-2">
                Incident: {new Date(report.incidentDate).toLocaleDateString()} · Reported:{' '}
                {new Date(report.createdAt).toLocaleDateString()}
                {report.officialReference && (
                  <>
                    {' '}· Ref {report.officialReference}
                    {report.officialReferenceVerified ? ' (verified)' : ' (unverified)'}
                  </>
                )}
              </p>

              <p className="text-sm text-gray-700 dark:text-gray-300">{report.description}</p>

              {report.status === 'Corroborated' && (
                <p className="mt-2 text-xs text-amber-700">
                  Publicly visible via clause 6.3 ({report.corroborationPath}).
                </p>
              )}

              {report.status === 'Pending' && (
                <DecisionPanel
                  isPending={decide.isPending}
                  onDecide={(decision, action) =>
                    decide.mutate({ id: report.id, decision, action })
                  }
                />
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
