import { useEffect, useRef, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Upload, PenLine, X, ScanSearch } from 'lucide-react'
import { reportsApi } from '@/api/reports'
import { verificationApi } from '@/api/verification'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { cn } from '@/lib/utils'

type ReportMode = 'upload' | 'manual'

const ACCEPTED = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024

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

const tabs: { id: ReportMode; label: string; icon: React.ReactNode }[] = [
  { id: 'upload', label: 'Upload Screenshots', icon: <Upload className="h-4 w-4" /> },
  { id: 'manual', label: 'Manual Entry', icon: <PenLine className="h-4 w-4" /> },
]

export function ReportDriverPage() {
  const location = useLocation()
  const prefill = location.state as { driverName?: string; registrationNumber?: string } | null

  const [mode, setMode] = useState<ReportMode>('upload')
  const [evidenceFiles, setEvidenceFiles] = useState<File[]>([])
  const [isDragging, setIsDragging] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { register, handleSubmit, setValue, reset, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { driverName: '', registrationNumber: '' },
  })

  const driverName = watch('driverName')
  const registrationNumber = watch('registrationNumber')

  useEffect(() => {
    if (prefill?.driverName) setValue('driverName', prefill.driverName)
    if (prefill?.registrationNumber) setValue('registrationNumber', prefill.registrationNumber)
  }, [])

  const addFiles = (incoming: FileList | null) => {
    if (!incoming) return
    const valid = Array.from(incoming).filter((f) => {
      if (!ACCEPTED.includes(f.type)) { toast.error(`${f.name}: unsupported format`); return false }
      if (f.size > MAX_SIZE) { toast.error(`${f.name}: exceeds 10MB limit`); return false }
      return true
    })
    setEvidenceFiles((prev) => [...prev, ...valid].slice(0, 3))
  }

  const removeFile = (i: number) => setEvidenceFiles((prev) => prev.filter((_, idx) => idx !== i))

  const scanMutation = useMutation({
    mutationFn: () => verificationApi.extractDriverInfo(evidenceFiles),
    onSuccess: (result) => {
      if (result.driverName) setValue('driverName', result.driverName, { shouldValidate: true })
      if (result.registrationNumber) setValue('registrationNumber', result.registrationNumber, { shouldValidate: true })
      toast.success('Fields populated from screenshot. Please review and complete the form.')
    },
    onError: () => toast.error('Could not extract driver info from the screenshot. Please fill in the fields manually.'),
  })

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      reportsApi.create({ ...data, evidence: mode === 'upload' ? evidenceFiles : undefined }),
    onSuccess: () => {
      toast.success('Report submitted successfully. Our moderators will review it.')
      reset()
      setEvidenceFiles([])
    },
    onError: (err: any) =>
      toast.error(err?.response?.data?.title ?? err?.message ?? 'Failed to submit report. Please try again.'),
  })

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Report a Driver</h1>
        <p className="text-gray-500 mt-1">Help keep the community safe by reporting unsafe behaviour.</p>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex gap-1 bg-gray-100 dark:bg-gray-800 rounded-lg p-1">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                type="button"
                onClick={() => setMode(tab.id)}
                className={cn(
                  'flex-1 flex items-center justify-center gap-2 py-2 px-3 rounded-md text-sm font-medium transition-colors',
                  mode === tab.id
                    ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm'
                    : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-300'
                )}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}
          </div>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-5">

            {mode === 'upload' && (
              <div className="space-y-2">
                <p className="text-sm text-gray-500">
                  Attach screenshots of the incident — driver profile, vehicle, or chat. Up to 3 images.
                </p>
                <div
                  className={cn(
                    'border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors',
                    isDragging ? 'border-blue-500 bg-blue-50 dark:bg-blue-950' : 'border-gray-300 dark:border-gray-600 hover:border-blue-400'
                  )}
                  onClick={() => fileInputRef.current?.click()}
                  onDragOver={(e) => { e.preventDefault(); setIsDragging(true) }}
                  onDragLeave={() => setIsDragging(false)}
                  onDrop={(e) => { e.preventDefault(); setIsDragging(false); addFiles(e.dataTransfer.files) }}
                >
                  <Upload className="h-10 w-10 text-gray-400 mx-auto mb-3" />
                  <p className="font-medium text-gray-700 dark:text-gray-300">Drop screenshots here or click to browse</p>
                  <p className="text-sm text-gray-400 mt-1">JPG, PNG, WebP — max 10MB each</p>
                  <input
                    ref={fileInputRef}
                    type="file"
                    multiple
                    accept={ACCEPTED.join(',')}
                    className="hidden"
                    onChange={(e) => { addFiles(e.target.files); e.target.value = '' }}
                  />
                </div>
                {evidenceFiles.length > 0 && (
                  <div className="space-y-2">
                    {evidenceFiles.map((f, i) => (
                      <div key={i} className="flex items-center justify-between bg-gray-50 dark:bg-gray-800 rounded-md px-3 py-2">
                        <span className="text-sm text-gray-700 dark:text-gray-300 truncate">{f.name}</span>
                        <button type="button" onClick={() => removeFile(i)} className="text-gray-400 hover:text-red-500 ml-2">
                          <X className="h-4 w-4" />
                        </button>
                      </div>
                    ))}
                    <Button
                      type="button"
                      variant="secondary"
                      className="w-full"
                      disabled={scanMutation.isPending}
                      onClick={() => scanMutation.mutate()}
                    >
                      <ScanSearch className="h-4 w-4 mr-2" />
                      {scanMutation.isPending ? 'Scanning…' : 'Scan Screenshot & Populate Fields'}
                    </Button>
                  </div>
                )}
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Driver Name</Label>
                <Input
                  placeholder="e.g. John Smith"
                  value={driverName}
                  onChange={(e) => setValue('driverName', e.target.value, { shouldValidate: true })}
                />
                {errors.driverName && <p className="text-xs text-red-500">{errors.driverName.message}</p>}
              </div>
              <div className="space-y-1">
                <Label>Registration Number</Label>
                <Input
                  placeholder="e.g. ABC123GP"
                  value={registrationNumber}
                  onChange={(e) => setValue('registrationNumber', e.target.value, { shouldValidate: true })}
                />
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

            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? 'Submitting…' : 'Submit Report'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
