import { useState, useRef } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Upload, X, ShieldCheck, AlertTriangle, Info, Flag } from 'lucide-react'
import { toast } from 'sonner'
import { Link } from 'react-router-dom'
import { verificationApi } from '@/api/verification'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import type { VerificationResult } from '@/types'
import { cn } from '@/lib/utils'

const ACCEPTED = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024

export function VerifyDriverPage() {
  const [files, setFiles] = useState<File[]>([])
  const [result, setResult] = useState<VerificationResult | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragging, setIsDragging] = useState(false)

  const mutation = useMutation({
    mutationFn: () => verificationApi.upload(files),
    onSuccess: (data) => { setResult(data); setFiles([]) },
    onError: () => toast.error('Verification failed. Please try again.'),
  })

  const addFiles = (incoming: FileList | null) => {
    if (!incoming) return
    const valid = Array.from(incoming).filter((f) => {
      if (!ACCEPTED.includes(f.type)) { toast.error(`${f.name}: unsupported format`); return false }
      if (f.size > MAX_SIZE) { toast.error(`${f.name}: exceeds 10MB limit`); return false }
      return true
    })
    setFiles((prev) => [...prev, ...valid].slice(0, 3))
  }

  const removeFile = (i: number) => setFiles((prev) => prev.filter((_, idx) => idx !== i))

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Verify a Driver</h1>
        <p className="text-gray-500 mt-1">Upload screenshots from your ride-hailing app to check a driver's safety record.</p>
      </div>

      {!result && (
        <Card>
          <CardHeader>
            <CardTitle>Upload Screenshots</CardTitle>
            <CardDescription>
              Upload up to 3 screenshots (driver profile, vehicle, or details). Supported: JPG, PNG, WEBP — max 10MB each.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div
              className={cn(
                'border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors',
                isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-blue-400'
              )}
              onClick={() => inputRef.current?.click()}
              onDragOver={(e) => { e.preventDefault(); setIsDragging(true) }}
              onDragLeave={() => setIsDragging(false)}
              onDrop={(e) => { e.preventDefault(); setIsDragging(false); addFiles(e.dataTransfer.files) }}
            >
              <Upload className="h-10 w-10 text-gray-400 mx-auto mb-3" />
              <p className="font-medium text-gray-700 dark:text-gray-300">Drop screenshots here or click to browse</p>
              <p className="text-sm text-gray-400 mt-1">Up to 3 files, max 10MB each</p>
              <input ref={inputRef} type="file" multiple accept={ACCEPTED.join(',')} className="hidden" onChange={(e) => addFiles(e.target.files)} />
            </div>

            {files.length > 0 && (
              <div className="space-y-2">
                {files.map((f, i) => (
                  <div key={i} className="flex items-center justify-between bg-gray-50 dark:bg-gray-800 rounded-md px-3 py-2">
                    <span className="text-sm text-gray-700 dark:text-gray-300 truncate">{f.name}</span>
                    <button onClick={() => removeFile(i)} className="text-gray-400 hover:text-red-500 ml-2">
                      <X className="h-4 w-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}

            <Button
              className="w-full"
              disabled={files.length === 0 || mutation.isPending}
              onClick={() => mutation.mutate()}
            >
              {mutation.isPending ? 'Verifying…' : 'Verify Driver'}
            </Button>
          </CardContent>
        </Card>
      )}

      {result && (
        <Card className={cn('border-2', result.status === 'Safe' ? 'border-green-200' : result.status === 'HighRisk' ? 'border-red-300' : 'border-orange-200')}>
          <CardHeader>
            <div className="flex items-start justify-between">
              <div>
                <CardTitle className="flex items-center gap-2">
                  {result.status === 'Safe'
                    ? <ShieldCheck className="h-5 w-5 text-green-600" />
                    : <AlertTriangle className="h-5 w-5 text-red-600" />}
                  Verification Result
                </CardTitle>
              </div>
              <RiskBadge status={result.status} />
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-500">Driver Name</p>
                <p className="font-semibold text-gray-900 dark:text-white">{result.driverName || '—'}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Registration</p>
                <p className="font-semibold text-gray-900 dark:text-white">{result.registrationNumber || '—'}</p>
              </div>
              {result.vehicleMake && (
                <div>
                  <p className="text-xs text-gray-500">Vehicle</p>
                  <p className="font-semibold text-gray-900 dark:text-white">{result.vehicleMake} {result.vehicleModel}</p>
                </div>
              )}
              <div>
                <p className="text-xs text-gray-500">Reports</p>
                <p className="font-semibold text-gray-900 dark:text-white">{result.reportCount}</p>
              </div>
            </div>
            <RiskScore score={result.riskScore} />
            {result.status !== 'Safe' && (
              <div className="flex items-start gap-2 bg-red-50 dark:bg-red-950 rounded-md p-3 text-sm text-red-700 dark:text-red-300">
                <Info className="h-4 w-4 mt-0.5 shrink-0" />
                <span>This driver has been flagged by the community. Consider cancelling your ride and requesting a new driver.</span>
              </div>
            )}
            <div className="flex gap-2">
              <Button variant="outline" className="flex-1" onClick={() => setResult(null)}>
                Verify Another Driver
              </Button>
              <Button asChild variant="destructive" className="flex-1">
                <Link
                  to="/passenger/report"
                  state={{ driverName: result.driverName, registrationNumber: result.registrationNumber }}
                >
                  <Flag className="h-4 w-4 mr-2" /> Report Driver
                </Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
