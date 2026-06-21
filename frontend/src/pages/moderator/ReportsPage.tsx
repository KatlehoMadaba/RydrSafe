import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { reportsApi } from '@/api/reports'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { CheckCircle, XCircle, ArrowUpCircle } from 'lucide-react'

const severityVariant = {
  Low: 'success' as const,
  Medium: 'warning' as const,
  High: 'destructive' as const,
  Critical: 'destructive' as const,
}

export function ModeratorReportsPage() {
  const [statusFilter, setStatusFilter] = useState('all')
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['reports', statusFilter],
    queryFn: () => reportsApi.getAll({ status: statusFilter === 'all' ? undefined : statusFilter, pageSize: 50 }),
  })

  const approve = useMutation({
    mutationFn: reportsApi.approve,
    onSuccess: () => { toast.success('Report approved'); qc.invalidateQueries({ queryKey: ['reports'] }) },
  })
  const reject = useMutation({
    mutationFn: (id: string) => reportsApi.reject(id),
    onSuccess: () => { toast.success('Report rejected'); qc.invalidateQueries({ queryKey: ['reports'] }) },
  })
  const escalate = useMutation({
    mutationFn: reportsApi.escalate,
    onSuccess: () => { toast.success('Report escalated'); qc.invalidateQueries({ queryKey: ['reports'] }) },
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Reports</h1>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
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

      <div className="space-y-3">
        {data?.items.map((report) => (
          <Card key={report.id}>
            <CardContent className="pt-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex flex-wrap items-center gap-2 mb-1">
                    <span className="font-semibold text-gray-900 dark:text-white">
                      {report.category.replace(/([A-Z])/g, ' $1').trim()}
                    </span>
                    <Badge variant={severityVariant[report.severity]}>{report.severity}</Badge>
                    <Badge variant={report.status === 'Pending' ? 'warning' : report.status === 'Approved' ? 'success' : 'secondary'}>
                      {report.status}
                    </Badge>
                  </div>
                  <p className="text-sm text-gray-500 mb-2">
                    Incident: {new Date(report.incidentDate).toLocaleDateString()} · Reported: {new Date(report.createdAt).toLocaleDateString()}
                  </p>
                  <p className="text-sm text-gray-700 dark:text-gray-300">{report.description}</p>
                </div>
                {report.status === 'Pending' && (
                  <div className="flex flex-col gap-2 shrink-0">
                    <Button size="sm" variant="outline" className="text-green-600 border-green-300 hover:bg-green-50"
                      onClick={() => approve.mutate(report.id)}>
                      <CheckCircle className="h-3 w-3 mr-1" />Approve
                    </Button>
                    <Button size="sm" variant="outline" className="text-red-600 border-red-300 hover:bg-red-50"
                      onClick={() => reject.mutate(report.id)}>
                      <XCircle className="h-3 w-3 mr-1" />Reject
                    </Button>
                    <Button size="sm" variant="outline" className="text-orange-600 border-orange-300 hover:bg-orange-50"
                      onClick={() => escalate.mutate(report.id)}>
                      <ArrowUpCircle className="h-3 w-3 mr-1" />Escalate
                    </Button>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
