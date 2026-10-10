import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Search, ChevronRight, Flag, Info, Clock, ShieldQuestion } from 'lucide-react'
import { reportsApi } from '@/api/reports'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { SEVERITY_TONE } from '@/lib/riskStatus'
import type { CommunityDriver, PublicReportIncident, CorroborationPath } from '@/types'

/** How a report reached `Corroborated` — a statement about our check, not a further allegation. */
const CORROBORATION_LABEL: Record<CorroborationPath, string> = {
  IndependentReports: 'Confirmed by independent reports from different passengers',
  OfficialReference: 'Confirmed against an official police reference',
  PublicRecord: 'Confirmed against a public record',
  None: 'Confirmed',
}

function formatPeriod(iso: string, precision: 'Month' | 'Year') {
  const d = new Date(iso)
  return precision === 'Year'
    ? String(d.getUTCFullYear())
    : d.toLocaleDateString(undefined, { month: 'long', year: 'numeric', timeZone: 'UTC' })
}

function categoryLabel(category: string) {
  return category.replace(/([A-Z])/g, ' $1').trim()
}

/**
 * Clause 6.4. What one user may learn about what other users reported: a category, a severity
 * band, how many reports were corroborated, and roughly when. Never a description, never a
 * reporter.
 *
 * Driver-scoped by design. A single scrollable list of every allegation against every named
 * driver is a blacklist, and publishing one is a different legal act from answering "what is
 * known about this driver" — see issue #43.
 *
 * The list leads with severity rather than the driver's status band, because the status band is
 * RydrSafe's own finding and this page is about what passengers reported. `reportCount` from the
 * driver list endpoint is not used anywhere here: it counts Pending, Rejected and Withdrawn
 * reports too, and clause 6.2 reserves public standing for corroborated ones. Every count on
 * this page comes from the community endpoint, which applies that filter server-side.
 */
export function CommunityReportsPage() {
  const [term, setTerm] = useState('')
  const [submitted, setSubmitted] = useState('')
  const [selected, setSelected] = useState<CommunityDriver | null>(null)
  const detailRef = useRef<HTMLDivElement>(null)

  const { data, isLoading: listLoading } = useQuery({
    queryKey: ['community-drivers', submitted],
    queryFn: () =>
      reportsApi.getCommunityDrivers({
        search: submitted || undefined,
        page: 1,
        pageSize: 20,
      }),
  })

  const { data: incidents, isLoading: loadingIncidents } = useQuery({
    queryKey: ['public-incidents', selected?.id],
    queryFn: () => reportsApi.getPublicIncidents(selected!.id),
    enabled: !!selected,
  })

  const drivers = data?.items ?? []

  // Picking a driver is a request to read their reports, and on a phone the card opens below
  // the fold. Scroll it into view rather than leaving the list looking unchanged.
  useEffect(() => {
    if (!selected) return
    detailRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }, [selected])

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
          You see what category of thing was reported, how serious the reporter said it was, how
          many reports were independently confirmed, and roughly when. You never see what anyone
          wrote, or who wrote it — a written account is where identifying and unproven detail
          lives. Reports still waiting on a moderator are counted, and labelled as unchecked.
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
          <CardDescription>
            Severity is what the passengers who reported chose, not a rating we gave the driver.
            Select a driver to read the reports behind it.
          </CardDescription>
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
                  aria-current={selected?.id === d.id ? 'true' : undefined}
                  className={
                    'flex w-full items-start gap-3 rounded-md border p-3 text-left transition-colors ' +
                    (selected?.id === d.id
                      ? 'border-primary bg-accent'
                      : 'border-border hover:bg-accent')
                  }
                >
                  <div className="min-w-0 flex-1 space-y-1.5">
                    <p className="truncate text-sm font-medium text-foreground">{d.driverName}</p>
                    <p className="truncate text-xs text-muted-foreground">
                      {d.registrationNumber ?? 'No registration on record'}
                    </p>

                    <div className="flex flex-wrap items-center gap-1.5">
                      {d.severities.map((s) => (
                        <Badge key={s.severityBand} variant={SEVERITY_TONE[s.severityBand]}>
                          {s.severityBand} · {s.count}
                        </Badge>
                      ))}

                      {d.pendingReportCount > 0 && (
                        <Badge variant="norecord" icon={Clock}>
                          {d.pendingReportCount} unchecked
                        </Badge>
                      )}

                      {d.severities.length === 0 && d.pendingReportCount === 0 && (
                        <Badge variant="outline" icon={ShieldQuestion}>
                          Nothing reported
                        </Badge>
                      )}
                    </div>

                    {/*
                      The disclaimer sits on the row itself, not only in the page banner. A
                      severity badge next to a named person reads as a verdict unless the
                      sentence denying that is in the same glance.
                    */}
                    {(d.severities.length > 0 || d.pendingReportCount > 0) && (
                      <p className="text-[11px] leading-snug text-subtle">
                        {d.pendingReportCount > 0 && d.severities.length === 0
                          ? 'Reported by passengers. No moderator has checked these yet — nothing here has been verified.'
                          : d.pendingReportCount > 0
                            ? `Severity as reported by passengers, not a moderator's finding. ${d.pendingReportCount} further report${d.pendingReportCount === 1 ? '' : 's'} not yet checked.`
                            : "Severity as reported by passengers, not a moderator's finding."}
                      </p>
                    )}
                  </div>

                  <ChevronRight className="mt-0.5 h-4 w-4 shrink-0 text-subtle" />
                </button>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {selected && (
        <Card ref={detailRef} className="scroll-mt-4">
          <CardHeader>
            <CardTitle className="text-base">Reports about {selected.driverName}</CardTitle>
            <CardDescription>
              {selected.registrationNumber ?? 'No registration on record'}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {selected.pendingReportCount > 0 && (
              <Alert>
                <Clock />
                <AlertTitle>
                  {selected.pendingReportCount} report
                  {selected.pendingReportCount === 1 ? '' : 's'} not yet checked by a moderator
                </AlertTitle>
                <AlertDescription>
                  {selected.pendingReportCount === 1 ? 'Someone has' : 'People have'} reported this
                  driver and nobody on our team has assessed{' '}
                  {selected.pendingReportCount === 1 ? 'it' : 'them'} yet. We tell you{' '}
                  {selected.pendingReportCount === 1 ? 'it' : 'they'} exist rather than leaving you
                  with silence, but that is all we can honestly say —{' '}
                  {selected.pendingReportCount === 1 ? 'it has' : 'they have'} no effect on this
                  driver's standing and{' '}
                  {selected.pendingReportCount === 1 ? 'does' : 'do'} not appear below.
                </AlertDescription>
              </Alert>
            )}

            {loadingIncidents && <LoadingSpinner />}

            {!loadingIncidents && (incidents?.length ?? 0) === 0 && (
              <p className="py-4 text-sm text-muted-foreground">
                Nothing confirmed against this driver. That does not mean nothing was reported —
                it means nothing has met the standard required to show it to you.
              </p>
            )}

            {!loadingIncidents && incidents && incidents.length > 0 && (
              <div className="space-y-3">
                <div>
                  <h2 className="text-sm font-semibold text-foreground">
                    {incidents.length} confirmed report{incidents.length === 1 ? '' : 's'} from
                    passengers
                  </h2>
                  <p className="text-xs text-muted-foreground">
                    One card per report. We show the category, how serious the reporter said it
                    was, and how we confirmed it — never what they wrote, and never who they are.
                  </p>
                </div>

                {incidents.map((incident, i) => (
                  <IncidentCard key={`${incident.category}-${incident.incidentPeriod}-${i}`} incident={incident} />
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

/** One corroborated report, in the clause 6.4 shape: no description, no reporter, no exact date. */
function IncidentCard({ incident }: { incident: PublicReportIncident }) {
  return (
    <div className="rounded-md border border-border p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-sm font-medium text-foreground">{categoryLabel(incident.category)}</p>
          <p className="text-xs text-muted-foreground">
            {formatPeriod(incident.incidentPeriod, incident.incidentPeriodPrecision)}
          </p>
        </div>
        <Badge variant={SEVERITY_TONE[incident.severityBand]}>{incident.severityBand}</Badge>
      </div>
      <p className="mt-2 text-xs text-subtle">
        {CORROBORATION_LABEL[incident.corroborationPath]}
      </p>
    </div>
  )
}
