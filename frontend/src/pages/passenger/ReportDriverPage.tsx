import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { toast } from 'sonner'
import { reportsApi } from '@/api/reports'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

const schema = z.object({
  driverName: z.string().min(2, 'Driver name is required'),
  registrationNumber: z.string().min(2, 'Registration number is required'),
  category: z.enum(['RecklessDriving', 'Harassment', 'Assault', 'Theft', 'Fraud', 'UnsafeVehicle', 'IntoxicatedDriving', 'Other']),
  severity: z.enum(['Low', 'Medium', 'High', 'Critical']),
  description: z.string().min(20, 'Please provide at least 20 characters of detail'),
  incidentDate: z.string().min(1, 'Incident date is required'),
  reportedToPolice: z.boolean(),
})
type FormData = z.infer<typeof schema>

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

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { reportedToPolice: false },
  })

  useEffect(() => {
    if (prefill?.driverName) setValue('driverName', prefill.driverName)
    if (prefill?.registrationNumber) setValue('registrationNumber', prefill.registrationNumber)
  }, [prefill?.driverName, prefill?.registrationNumber, setValue])

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
          <CardDescription>All reports are reviewed by our moderation team before being published.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-5">
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

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label htmlFor="category">Category</Label>
                <Select onValueChange={(v) => setValue('category', v as FormData['category'])}>
                  <SelectTrigger id="category">
                    <SelectValue placeholder="Select category" />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((c) => (
                      <SelectItem key={c.value} value={c.value}>
                        {c.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.category && <p className="text-xs text-highrisk">{errors.category.message}</p>}
              </div>
              <div className="space-y-1">
                <Label htmlFor="severity">Severity</Label>
                <Select onValueChange={(v) => setValue('severity', v as FormData['severity'])}>
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
                {errors.severity && <p className="text-xs text-highrisk">{errors.severity.message}</p>}
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="incidentDate">Incident Date</Label>
              <Input id="incidentDate" type="date" max={new Date().toISOString().split('T')[0]} {...register('incidentDate')} />
              {errors.incidentDate && <p className="text-xs text-highrisk">{errors.incidentDate.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="description">Description</Label>
              <Textarea id="description" rows={5} placeholder="Describe what happened in detail…" {...register('description')} />
              {errors.description && <p className="text-xs text-highrisk">{errors.description.message}</p>}
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

            <Button type="submit" className="w-full" isLoading={isSubmitting || mutation.isPending}>
              Submit Report
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
