import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'
import { useForm, Controller, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { toast } from 'sonner'
import { reportsApi } from '@/api/reports'
import { platformApi } from '@/api/platform'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { AlertTriangle, EyeOff, Info } from 'lucide-react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

const schema = z.object({
  driverName: z.string().min(2, 'Driver name is required'),
  registrationNumber: z.string().min(2, 'Registration number is required'),
  category: z.enum(['RecklessDriving', 'Harassment', 'Assault', 'Theft', 'Fraud', 'UnsafeVehicle', 'IntoxicatedDriving', 'Other']),
  severity: z.enum(['Low', 'Medium', 'High', 'Critical']),
  description: z.string().min(20, 'Please provide at least 20 characters of detail'),
  // Someone reporting a months-old incident often cannot name the day, and forcing a guess puts
  // a date on the record that nobody actually claimed. Let them say how much they remember.
  incidentDatePrecision: z.enum(['Day', 'Month', 'Year']),
  incidentDate: z.string().min(1, 'Incident date is required'),
  reportedToPolice: z.boolean(),
  // Clause 6.4. Withholds the reporter's name from the moderation queue; the report stays
  // linked to the account underneath, which the copy below says plainly.
  isAnonymous: z.boolean(),
  // Clause 6.3(b). Optional, but supplying one is the fastest route to corroboration.
  officialReference: z.string().max(100).optional(),
})
type FormData = z.infer<typeof schema>

/**
 * The three inputs return different shapes — "2026-09-14", "2026-09", "2026". The API takes one
 * DateTime, so each is widened to the first instant of the period the reporter actually named.
 * The precision travels with it, so nothing downstream mistakes 1 January for a claimed date.
 */
function toIncidentDate(value: string, precision: FormData['incidentDatePrecision']): string {
  if (precision === 'Year') return `${value.padStart(4, '0')}-01-01`
  if (precision === 'Month') return `${value}-01`
  return value
}

const categories: { value: FormData['category']; label: string }[] = [
  { value: 'RecklessDriving', label: 'Reckless Driving' },
  { value: 'Harassment', label: 'Harassment' },
  { value: 'Assault', label: 'Assault' },
  { value: 'Theft', label: 'Theft' },
  { value: 'Fraud', label: 'Fraud' },
  { value: 'UnsafeVehicle', label: 'Unsafe Vehicle' },
  { value: 'IntoxicatedDriving', label: 'Intoxicated Driving' },
  { value: 'Other', label: 'Other' },
]

const severities: { value: FormData['severity']; label: string; color: string }[] = [
  { value: 'Low', label: 'Low', color: 'text-safe' },
  { value: 'Medium', label: 'Medium', color: 'text-review' },
  { value: 'High', label: 'High', color: 'text-flagged' },
  { value: 'Critical', label: 'Critical', color: 'text-highrisk' },
]

export function ReportDriverPage() {
  const location = useLocation()
  const prefill = location.state as { driverName?: string; registrationNumber?: string } | null

  // Which categories the platform is currently accepting. Categories that are switched off are
  // left out of the list entirely rather than shown greyed: offering someone "Assault" and then
  // refusing the click reads as a broken form. The banner above the form is what stops that
  // being a silent omission — it names what is missing and why.
  const { data: config } = useQuery({
    queryKey: ['platform-config'],
    queryFn: platformApi.getConfig,
    staleTime: 5 * 60_000,
  })

  const unavailable = new Set(
    (config?.reportCategories ?? []).filter((c) => !c.available).map((c) => c.value),
  )

  // Before config arrives there is nothing to filter against, so show the full list rather than
  // flashing an empty dropdown.
  const availableCategories = config
    ? categories.filter((c) => !unavailable.has(c.value))
    : categories

  const onlyCategory = availableCategories.length === 1 ? availableCategories[0] : null

  const {
    register,
    handleSubmit,
    setValue,
    control,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { reportedToPolice: false, isAnonymous: false, incidentDatePrecision: 'Day' },
  })

  // useWatch rather than watch(): it subscribes this one value instead of re-rendering the whole
  // form on every keystroke, which is the concern noted on the Controller usage below.
  const datePrecision = useWatch({ control, name: 'incidentDatePrecision' }) ?? 'Day'

  useEffect(() => {
    if (prefill?.driverName) setValue('driverName', prefill.driverName)
    if (prefill?.registrationNumber) setValue('registrationNumber', prefill.registrationNumber)
  }, [prefill?.driverName, prefill?.registrationNumber, setValue])

  // With the Category A standstill in force only one category is left, and asking someone to
  // pick it from a list of one is a step that exists for no reason. Select it for them; the
  // field stays visible so the report still says what it is.
  useEffect(() => {
    if (onlyCategory) setValue('category', onlyCategory.value, { shouldValidate: false })
  }, [onlyCategory, setValue])

  const mutation = useMutation({
    mutationFn: reportsApi.create,
    onSuccess: () => {
      toast.success('Report submitted successfully. Our moderators will review it.')
      reset()
    },
    onError: (err: unknown) => {
      const message = isAxiosError<{ title?: string }>(err) ? err.response?.data?.title : undefined
      toast.error(message ?? 'Failed to submit report. Please try again.')
    },
  })

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-foreground">Report a Driver</h1>
        <p className="text-muted-foreground mt-1">Help keep the community safe by reporting unsafe behaviour.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Incident Details</CardTitle>
          <CardDescription>Reports are reviewed by a moderator. A serious allegation is not shown to other users unless it is independently corroborated (clause 6.3).</CardDescription>
        </CardHeader>
        <CardContent>
          {config && !config.categoryAProcessingEnabled && (
            <Alert variant="review" className="mb-5">
              <AlertTriangle />
              <AlertTitle>Why some options are greyed out</AlertTitle>
              <AlertDescription>{config.categoryADisabledReason}</AlertDescription>
            </Alert>
          )}

          {/*
            Clause 7 and clause 8. A reporter who believes they are filing a criminal charge
            writes differently — and worse — than one who understands they are describing an
            experience. Saying so before the form rather than in the terms is the difference
            between a disclosure and a disclaimer.
          */}
          <Alert className="mb-5">
            <Info />
            <AlertTitle>You are describing what happened, not laying a charge</AlertTitle>
            <AlertDescription>
              Tell us what you experienced, in your own words. This is your account of a trip —
              it is not a criminal charge, and submitting it does not mean the driver has been
              found guilty of anything. A moderator reads every report, and nothing you write
              here is shown to other users unless it is independently corroborated. If a crime
              has been committed, report it to SAPS on 10111 as well.
            </AlertDescription>
          </Alert>

          <form
            onSubmit={handleSubmit((d) =>
              mutation.mutate({
                ...d,
                incidentDate: toIncidentDate(d.incidentDate, d.incidentDatePrecision),
              }),
            )}
            className="space-y-5"
          >
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label htmlFor="driverName">Driver Name</Label>
                <Input id="driverName" placeholder="e.g. John Smith" {...register('driverName')} />
                {errors.driverName && <p className="text-xs text-highrisk">{errors.driverName.message}</p>}
              </div>
              <div className="space-y-1">
                <Label htmlFor="registrationNumber">Registration Number</Label>
                <Input id="registrationNumber" placeholder="e.g. ABC123GP" {...register('registrationNumber')} />
                {errors.registrationNumber && <p className="text-xs text-highrisk">{errors.registrationNumber.message}</p>}
              </div>
            </div>

            <div
              className={
                onlyCategory ? 'grid grid-cols-1 gap-4' : 'grid grid-cols-1 sm:grid-cols-2 gap-4'
              }
            >
              {/* Controller rather than watch(): Radix's Select needs a controlled `value`
                  so reset() clears it, and watch() defeats React Compiler memoisation.

                  The field disappears entirely while only one category is on offer — a select
                  with a single option is a decision the user does not get to make. The value is
                  still set on the form, so the report is categorised exactly as before, and the
                  banner above says which type is being accepted. The moment the Category A gate
                  opens, `onlyCategory` goes null and the dropdown comes back on its own. */}
              {!onlyCategory && (
                <div className="space-y-1">
                  <Label htmlFor="category">Category</Label>
                  <Controller
                    control={control}
                    name="category"
                    render={({ field }) => (
                      <Select value={field.value ?? ''} onValueChange={field.onChange}>
                        <SelectTrigger id="category">
                          <SelectValue placeholder="Select category" />
                        </SelectTrigger>
                        <SelectContent>
                          {availableCategories.map((c) => (
                            <SelectItem key={c.value} value={c.value}>
                              {c.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                  {config && !config.categoryAProcessingEnabled && (
                    <p className="text-xs text-muted-foreground">
                      Report types that accuse a driver of a crime are not listed. We need the
                      Information Regulator&apos;s written permission before we may collect those,
                      and we do not have it yet.
                    </p>
                  )}
                  {errors.category && (
                    <p className="text-xs text-highrisk">{errors.category.message}</p>
                  )}
                </div>
              )}
              <div className="space-y-1">
                <Label htmlFor="severity">Severity</Label>
                <Controller
                  control={control}
                  name="severity"
                  render={({ field }) => (
                    <Select value={field.value ?? ''} onValueChange={field.onChange}>
                      <SelectTrigger id="severity">
                        <SelectValue placeholder="Select severity" />
                      </SelectTrigger>
                      <SelectContent>
                        {severities.map((s) => (
                          <SelectItem key={s.value} value={s.value}>
                            <span className={s.color}>{s.label}</span>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
                {errors.severity && <p className="text-xs text-highrisk">{errors.severity.message}</p>}
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="incidentDate">When did this happen?</Label>

              {/* Three native inputs rather than one, because the browser's own month and year
                  pickers are better than anything rebuilt here, and each returns exactly the
                  granularity it names. */}
              <div className="flex flex-wrap gap-2">
                {(
                  [
                    { value: 'Day', label: 'I know the date' },
                    { value: 'Month', label: 'Only the month' },
                    { value: 'Year', label: 'Only the year' },
                  ] as const
                ).map((option) => (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => {
                      setValue('incidentDatePrecision', option.value)
                      // The formats are not interchangeable, so a half-typed value from the
                      // previous mode would submit as an invalid date.
                      setValue('incidentDate', '')
                    }}
                    className={
                      datePrecision === option.value
                        ? 'rounded-full border border-primary bg-primary px-3 py-1 text-xs font-medium text-primary-foreground'
                        : 'rounded-full border border-border px-3 py-1 text-xs text-muted-foreground hover:bg-accent'
                    }
                  >
                    {option.label}
                  </button>
                ))}
              </div>

              {/* en-CA gives YYYY-MM-DD in the user's own timezone; toISOString() would
                  use UTC and block "today" for anyone east of it. */}
              {datePrecision === 'Day' && (
                <Input
                  id="incidentDate"
                  type="date"
                  max={new Date().toLocaleDateString('en-CA')}
                  {...register('incidentDate')}
                />
              )}
              {datePrecision === 'Month' && (
                <Input
                  id="incidentDate"
                  type="month"
                  max={new Date().toLocaleDateString('en-CA').slice(0, 7)}
                  {...register('incidentDate')}
                />
              )}
              {datePrecision === 'Year' && (
                <Input
                  id="incidentDate"
                  type="number"
                  inputMode="numeric"
                  placeholder="e.g. 2026"
                  min={1990}
                  max={new Date().getFullYear()}
                  {...register('incidentDate')}
                />
              )}

              <p className="text-xs text-muted-foreground">
                {datePrecision === 'Day'
                  ? 'If you are not sure of the exact day, say so above rather than guessing.'
                  : 'Recorded as approximate. A moderator sees that you gave a ' +
                    (datePrecision === 'Month' ? 'month' : 'year') +
                    ', not an exact date.'}
              </p>
              {errors.incidentDate && <p className="text-xs text-highrisk">{errors.incidentDate.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="description">Description</Label>
              <Textarea id="description" rows={5} placeholder="Describe what happened in detail…" {...register('description')} />
              {errors.description && <p className="text-xs text-highrisk">{errors.description.message}</p>}
            </div>

            <div className="space-y-1">
              <Label>SAPS case number (optional)</Label>
              <Input placeholder="e.g. CAS 123/01/2026" {...register('officialReference')} />
              <p className="text-xs text-muted-foreground">
                If you opened a case, the reference lets a moderator corroborate your report on its
                own. Without one, a serious allegation stays private until a second, independent
                report names the same driver.
              </p>
              {errors.officialReference && (
                <p className="text-xs text-highrisk">{errors.officialReference.message}</p>
              )}
            </div>

            <div className="flex items-start gap-2 rounded-md bg-muted p-3">
              <input
                id="reportedToPolice"
                type="checkbox"
                className="mt-0.5 h-4 w-4 rounded border-input text-primary focus:ring-ring"
                {...register('reportedToPolice')}
              />
              <Label htmlFor="reportedToPolice" className="text-sm font-normal text-foreground">
                I have also reported this incident to the police.
                <span className="block text-xs text-muted-foreground">
                  Drivers with multiple reports that have been taken to the police are marked as high risk.
                </span>
              </Label>
            </div>

            <div className="space-y-2 rounded-md border border-border p-3">
              <div className="flex items-start gap-2">
                <input
                  id="isAnonymous"
                  type="checkbox"
                  className="mt-0.5 h-4 w-4 rounded border-input text-primary focus:ring-ring"
                  {...register('isAnonymous')}
                />
                <Label htmlFor="isAnonymous" className="text-sm font-normal text-foreground">
                  <span className="flex items-center gap-1.5">
                    <EyeOff className="h-3.5 w-3.5 text-subtle" />
                    Submit this report anonymously
                  </span>
                  <span className="block text-xs text-muted-foreground">
                    Your name is withheld from the report. Other users never see who reported a
                    driver in any case (clause 6.4).
                  </span>
                </Label>
              </div>

              <p className="text-xs text-muted-foreground">
                <strong className="font-medium text-foreground">Be clear about what this does.</strong>{' '}
                The report stays linked to your account in our database. It has to: two reports
                from one account cannot corroborate each other (clause 6.3(a)), you need the link
                to withdraw your own report, and it is how we deal with people who abuse the
                platform (clause 37). If you would rather we never held your real identity at all,
                create your account with a disposable address from a service such as{' '}
                <a
                  href="https://temp-mail.org/en/"
                  target="_blank"
                  rel="noreferrer noopener"
                  className="text-teal-600 underline-offset-4 hover:underline"
                >
                  temp-mail.org
                </a>
                . Note that you will not be able to recover that account if you lose the mailbox.
              </p>
            </div>

            <Button type="submit" className="w-full" isLoading={isSubmitting || mutation.isPending}>
              Submit Report
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
