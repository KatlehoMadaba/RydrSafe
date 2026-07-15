import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { reportsApi } from '@/api/reports'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ErrorState } from '@/components/ErrorState'
import { CheckCircle, XCircle, ArrowUpCircle, FileText } from 'lucide-react'

const severityVariant = {
  Low: 'success' as const,
  Medium: 'warning' as const,
  High: 'destructive' as const,
  Critical: 'destructive' as const,
}

export function ModeratorReportsPage() {
  const [statusFilter, setStatusFilter] = useState('all')
  const qc = useQueryClient()

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['reports', statusFilter],
    queryFn: () => reportsApi.getAll({ status: statusFilter === 'all' ? undefined : statusFilter, pageSize: 50 }),
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['reports'] })

  const approve = useMutation({
    mutationFn: reportsApi.approve,
    onSuccess: () => {
      toast.success('Report approved')
      invalidate()
    },
    onError: () => toast.error('Could not approve report. Please try again.'),
  })
  const reject = useMutation({
    mutationFn: (id: string) => reportsApi.reject(id),
    onSuccess: () => {
      toast.success('Report rejected')
      invalidate()
    },
    onError: () => toast.error('Could not reject report. Please try again.'),
  })
  const escalate = useMutation({
    mutationFn: reportsApi.escalate,
    onSuccess: () => {
      toast.success('Report escalated')
      invalidate()
    },
    onError: () => toast.error('Could not escalate report. Please try again.'),
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-display text-2xl font-bold text-foreground">Reports</h1>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All</SelectItem>
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="Approved">Approved</SelectItem>
            <SelectItem value="Rejected">Rejected</SelectItem>
            <SelectItem value="Escalated">Escalated</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}
      {isError && <ErrorState message="Couldn't load reports." onRetry={() => refetch()} />}

      {!isLoading && !isError && data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <FileText className="h-10 w-10 text-subtle mx-auto mb-3" />
            <p className="text-muted-foreground">No reports match this filter.</p>
          </CardContent>
        </Card>
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Driver</TableHead>
                  <TableHead>Report</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Dates</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((report) => {
                  // One in-flight action locks the whole row — the three verdicts are
                  // mutually exclusive, so a second click before the first settles is never intended.
                  const rowBusy =
                    (approve.isPending && approve.variables === report.id) ||
                    (reject.isPending && reject.variables === report.id) ||
                    (escalate.isPending && escalate.variables === report.id)

                  return (
                  <TableRow key={report.id}>
                    <TableCell>
                      <span className="font-medium text-foreground">{report.driverName || '—'}</span>
                    </TableCell>
                    <TableCell className="whitespace-normal max-w-md">
                      <div className="flex flex-wrap items-center gap-2 mb-1">
                        <span className="font-semibold text-foreground">{report.category.replace(/([A-Z])/g, ' $1').trim()}</span>
                        <Badge variant={severityVariant[report.severity]}>{report.severity}</Badge>
                        {report.reportedToPolice && <Badge variant="destructive">Police-reported</Badge>}
                      </div>
                      <p className="text-sm text-muted-foreground">{report.description}</p>
                    </TableCell>
                    <TableCell>
                      <Badge variant={report.status === 'Pending' ? 'warning' : report.status === 'Approved' ? 'success' : 'secondary'}>
                        {report.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="whitespace-normal text-xs text-muted-foreground">
                      Incident: {new Date(report.incidentDate).toLocaleDateString()}
                      <br />
                      Reported: {new Date(report.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="text-right">
                      {report.status === 'Pending' ? (
                        <div className="flex flex-col items-end gap-1.5">
                          <Button
                            size="sm"
                            variant="outline"
                            className="text-safe border-safe-muted hover:bg-safe-soft"
                            isLoading={approve.isPending && approve.variables === report.id}
                            disabled={rowBusy}
                            onClick={() => approve.mutate(report.id)}
                          >
                            <CheckCircle className="h-3 w-3 mr-1" />
                            Approve
                          </Button>
                          <Button
                            size="sm"
                            variant="outline"
                            className="text-highrisk border-highrisk-muted hover:bg-highrisk-soft"
                            isLoading={reject.isPending && reject.variables === report.id}
                            disabled={rowBusy}
                            onClick={() => reject.mutate(report.id)}
                          >
                            <XCircle className="h-3 w-3 mr-1" />
                            Reject
                          </Button>
                          <Button
                            size="sm"
                            variant="outline"
                            className="text-flagged border-flagged-muted hover:bg-flagged-soft"
                            isLoading={escalate.isPending && escalate.variables === report.id}
                            disabled={rowBusy}
                            onClick={() => escalate.mutate(report.id)}
                          >
                            <ArrowUpCircle className="h-3 w-3 mr-1" />
                            Escalate
                          </Button>
                        </div>
                      ) : (
                        <span className="text-xs text-subtle">—</span>
                      )}
                    </TableCell>
                  </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
