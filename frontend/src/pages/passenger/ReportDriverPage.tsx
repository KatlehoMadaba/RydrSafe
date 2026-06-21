import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation } from '@tanstack/react-query'
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
  { value: 'Low', label: 'Low', color: 'text-green-600' },
  { value: 'Medium', label: 'Medium', color: 'text-yellow-600' },
  { value: 'High', label: 'High', color: 'text-orange-600' },
  { value: 'Critical', label: 'Critical', color: 'text-red-600' },
]

export function ReportDriverPage() {
  const { register, handleSubmit, setValue, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: reportsApi.create,
    onSuccess: () => { toast.success('Report submitted successfully. Our moderators will review it.'); reset() },
    onError: () => toast.error('Failed to submit report. Please try again.'),
  })

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Report a Driver</h1>
        <p className="text-gray-500 mt-1">Help keep the community safe by reporting unsafe behaviour.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Incident Details</CardTitle>
          <CardDescription>All reports are reviewed by our moderation team before being published.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Driver Name</Label>
                <Input placeholder="e.g. John Smith" {...register('driverName')} />
                {errors.driverName && <p className="text-xs text-red-500">{errors.driverName.message}</p>}
              </div>
              <div className="space-y-1">
                <Label>Registration Number</Label>
                <Input placeholder="e.g. ABC123GP" {...register('registrationNumber')} />
                {errors.registrationNumber && <p className="text-xs text-red-500">{errors.registrationNumber.message}</p>}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Category</Label>
                <Select onValueChange={(v) => setValue('category', v as FormData['category'])}>
                  <SelectTrigger><SelectValue placeholder="Select category" /></SelectTrigger>
                  <SelectContent>
                    {categories.map((c) => <SelectItem key={c.value} value={c.value}>{c.label}</SelectItem>)}
                  </SelectContent>
                </Select>
                {errors.category && <p className="text-xs text-red-500">{errors.category.message}</p>}
              </div>
              <div className="space-y-1">
                <Label>Severity</Label>
                <Select onValueChange={(v) => setValue('severity', v as FormData['severity'])}>
                  <SelectTrigger><SelectValue placeholder="Select severity" /></SelectTrigger>
                  <SelectContent>
                    {severities.map((s) => (
                      <SelectItem key={s.value} value={s.value}>
                        <span className={s.color}>{s.label}</span>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.severity && <p className="text-xs text-red-500">{errors.severity.message}</p>}
              </div>
            </div>

            <div className="space-y-1">
              <Label>Incident Date</Label>
              <Input type="date" max={new Date().toISOString().split('T')[0]} {...register('incidentDate')} />
              {errors.incidentDate && <p className="text-xs text-red-500">{errors.incidentDate.message}</p>}
            </div>

            <div className="space-y-1">
              <Label>Description</Label>
              <Textarea rows={5} placeholder="Describe what happened in detail…" {...register('description')} />
              {errors.description && <p className="text-xs text-red-500">{errors.description.message}</p>}
            </div>

            <Button type="submit" className="w-full" disabled={isSubmitting || mutation.isPending}>
              {mutation.isPending ? 'Submitting…' : 'Submit Report'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
