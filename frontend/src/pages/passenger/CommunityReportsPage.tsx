import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Search, ChevronRight, Flag, Info } from 'lucide-react'
import { driversApi } from '@/api/drivers'
import { reportsApi } from '@/api/reports'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { RiskBadge } from '@/components/RiskBadge'
import type { DriverListItem } from '@/types'

const severityVariant: Record<string, 'success' | 'warning' | 'destructive'> = {
  Low: 'success',
  Medium: 'warning',
  High: 'destructive',
  Critical: 'destructive',
}

function formatRange(earliest?: string | null, latest?: string | null) {
  if (!earliest) return null
  const from = new Date(earliest).toLocaleDateString(undefined, { month: 'short', year: 'numeric' })
  const to = latest ? new Date(latest).toLocaleDateString(undefined, { month: 'short', year: 'numeric' }) : null
  return to && to !== from ? `${from} – ${to}` : from
}

/**
 * Clause 6.4. What one user may learn about what other users reported: a category, a severity
 * band, a count of corroborated reports, and a date range. Never a description, never a reporter,
 * never a per-report record.
 *
 * Driver-scoped by design. A single scrollable list of every allegation against every named
 * driver is a blacklist, and publishing one is a different legal act from answering "what is
 * known about this driver" — see issue #43.
 *
 * The `reportCount` on the driver list is deliberately not shown: it counts every report
 * including Pending, Rejected and Withdrawn, and clause 6.2 reserves public standing for
 * corroborated reports alone. The counts here come from the public-summary endpoint instead.
 */
export function CommunityReportsPage() {
  const [term, setTerm] = useState('')
  const [submitted, setSubmitted] = useState('')
  const [selected, setSelected] = useState<DriverListItem | null>(null)

  const { data: browse, isLoading: browsing } = useQuery({
    queryKey: ['drivers-browse'],
    queryFn: () => driversApi.getAll({ page: 1, pageSize: 20 }),
    enabled: submitted.length === 0,
  })

  const { data: found, isLoading: searching } = useQuery({
    queryKey: ['drivers-search', submitted],
    queryFn: () => driversApi.search(submitted),
    enabled: submitted.length > 0,
  })

  const { data: summaries, isLoading: loadingSummaries } = useQuery({
    queryKey: ['public-summary', selected?.id],
    queryFn: () => reportsApi.getPublicSummary(selected!.id),
    enabled: !!selected,
  })

  const drivers: DriverListItem[] = submitted ? (found ?? []) : (browse?.items ?? [])
  const listLoading = submitted ? searching : browsing

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-foreground">Community Reports</h1>
        <p className="mt-1 text-muted-foreground">
          What passengers have reported about a driver, in the only form we are allowed to show it.
        </p>
      </div>

      <Alert>
        <Info />
        <AlertTitle>What you can and cannot see here</AlertTitle>
        <AlertDescription>
          You see the category of what was reported, a severity band, how many reports were
          independently confirmed, and roughly when. You never see what anyone wrote, or who
          wrote it. Reports still awaiting review are not shown at all — only reports that have
          been confirmed under clause 6.3 appear.
        </AlertDescription>
      </Alert>

      <Card>
        <CardContent className="pt-6">
          <form
            onSubmit={(e) => {
              e.preventDefault()
              setSelected(null)
              setSubmitted(term.trim())
            }}
            className="space-y-2"
          >
            <Label htmlFor="driverSearch">Find a driver</Label>
            <div className="flex gap-2">
              <Input
                id="driverSearch"
                placeholder="Registration number or name"
                value={term}
                onChange={(e) => setTerm(e.target.value)}
              />
              <Button type="submit">
                <Search className="h-4 w-4" />
                Search
              </Button>
            </div>
            {submitted && (
              <button
                type="button"
                className="text-xs text-teal-600 underline-offset-4 hover:underline"
                onClick={() => {
                  setTerm('')
                  setSubmitted('')
                  setSelected(null)
                }}
              >
                Clear search
              </button>
            )}
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">
            {submitted ? `Results for “${submitted}”` : 'Recently added drivers'}
          </CardTitle>
          <CardDescription>Select a driver to see what has been reported.</CardDescription>
        </CardHeader>
        <CardContent>
          {listLoading && <LoadingSpinner />}

          {!listLoading && drivers.length === 0 && (
            <p className="py-6 text-center text-sm text-muted-foreground">
              {submitted ? 'No driver matched that search.' : 'No drivers on record yet.'}
            </p>
          )}

          {!listLoading && drivers.length > 0 && (
            <div className="space-y-1">
              {drivers.map((d) => (
                <button
                  key={d.id}
                  type="button"
                  onClick={() => setSelected(d)}
                  className={
                    'flex w-full items-center gap-3 rounded-md border p-3 text-left transition-colors ' +
                    (selected?.id === d.id
                      ? 'border-primary bg-accent'
                      : 'border-border hover:bg-accent')
                  }
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-foreground">{d.driverName}</p>
                    <p className="truncate text-xs text-muted-foreground">
                      {d.registrationNumber ?? 'No registration on record'}
                    </p>
                  </div>
                  <RiskBadge state={d.status} />
                  <ChevronRight className="h-4 w-4 shrink-0 text-subtle" />
                </button>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {selected && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{selected.driverName}</CardTitle>
            <CardDescription>
              {selected.registrationNumber ?? 'No registration on record'}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {loadingSummaries && <LoadingSpinner />}

            {!loadingSummaries && (summaries?.length ?? 0) === 0 && (
              <p className="py-4 text-sm text-muted-foreground">
                Nothing confirmed against this driver. That does not mean nothing was reported —
                it means nothing has met the standard required to show it to you.
              </p>
            )}

            {!loadingSummaries && summaries && summaries.length > 0 && (
              <div className="space-y-2">
                {summaries.map((s) => (
                  <div
                    key={s.category}
                    className="flex items-start justify-between gap-3 rounded-md border border-border p-3"
                  >
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-foreground">
                        {s.category.replace(/([A-Z])/g, ' $1').trim()}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {s.corroboratedCount} confirmed report{s.corroboratedCount === 1 ? '' : 's'}
                        {formatRange(s.earliestIncident, s.latestIncident) &&
                          ` · ${formatRange(s.earliestIncident, s.latestIncident)}`}
                      </p>
                    </div>
                    <Badge variant={severityVariant[s.severityBand] ?? 'secondary'}>
                      {s.severityBand}
                    </Badge>
                  </div>
                ))}
              </div>
            )}

            <div className="flex flex-wrap gap-2 border-t border-border pt-4">
              <Button asChild variant="outline" size="sm">
                <Link to="/passenger/verify">Verify this driver</Link>
              </Button>
              <Button asChild variant="destructive" size="sm">
                <Link
                  to="/passenger/report"
                  state={{
                    driverName: selected.driverName,
                    registrationNumber: selected.registrationNumber ?? '',
                  }}
                >
                  <Flag className="h-4 w-4" />
                  Report this driver
                </Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
