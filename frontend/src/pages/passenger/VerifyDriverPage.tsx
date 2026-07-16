import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Upload, PenLine, Flag } from 'lucide-react'
import { toast } from 'sonner'
import { Link } from 'react-router-dom'
import { verificationApi } from '@/api/verification'
import { useAuth } from '@/hooks/useAuth'
import { Card, CardContent, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs'
import { ScreenshotDropzone, ScreenshotThumbnails } from '@/components/verify/ScreenshotDropzone'
import { ManualLookupForm } from '@/components/verify/ManualLookupForm'
import { VerifyStepper } from '@/components/verify/VerifyStepper'
import { VerificationScanning } from '@/components/verify/VerificationScanning'
import { VerificationResultPanel } from '@/components/verify/VerificationResultPanel'
import { FollowDriverButton } from '@/components/verify/FollowDriverButton'
import type { VerificationResult } from '@/types'

type VerifyMode = 'screenshot' | 'manual'
type Phase = 'upload' | 'confirm' | 'scanning' | 'result'

const ACCEPTED = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024
const MAX_FILES = 3

// Both /verify (public) and /passenger/verify render this same component — the
// role branching below must keep working for both an anonymous visitor and a
// signed-in passenger.
export function VerifyDriverPage() {
  const [phase, setPhase] = useState<Phase>('upload')
  const [mode, setMode] = useState<VerifyMode>('screenshot')
  const [result, setResult] = useState<VerificationResult | null>(null)

  const [files, setFiles] = useState<File[]>([])
  const [regNumber, setRegNumber] = useState('')
  const [driverName, setDriverName] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')

  const { user } = useAuth()

  const onSuccess = (data: VerificationResult) => {
    setResult(data)
    setPhase('result')
  }
  const onError = () => {
    toast.error('Verification failed. Please try again.')
    setPhase('confirm')
  }

  const uploadMutation = useMutation({ mutationFn: () => verificationApi.upload(files), onSuccess, onError })
  const manualMutation = useMutation({
    mutationFn: () =>
      verificationApi.verifyManual({
        registrationNumber: regNumber.trim() || undefined,
        driverName: driverName.trim() || undefined,
        phoneNumber: phoneNumber.trim() || undefined,
      }),
    onSuccess,
    onError,
  })

  const isPassenger = user?.role === 'passenger'
  const canFollow = !!(user && result?.driverId && (result.status === 'Flagged' || result.status === 'HighRisk'))

  const addFiles = (incoming: FileList | null) => {
    if (!incoming) return
    const valid = Array.from(incoming).filter((f) => {
      if (!ACCEPTED.includes(f.type)) {
        toast.error(`${f.name}: unsupported format`)
        return false
      }
      if (f.size > MAX_SIZE) {
        toast.error(`${f.name}: exceeds 10MB limit`)
        return false
      }
      return true
    })
    setFiles((prev) => {
      const next = [...prev, ...valid]
      // Say so rather than silently dropping the extras, like the checks above do.
      if (next.length > MAX_FILES) toast.error(`Only the first ${MAX_FILES} screenshots are kept.`)
      return next.slice(0, MAX_FILES)
    })
  }
  const removeFile = (i: number) => setFiles((prev) => prev.filter((_, idx) => idx !== i))

  const canContinue = mode === 'screenshot' ? files.length > 0 : regNumber.trim().length > 0

  const submitVerification = () => {
    setPhase('scanning')
    if (mode === 'screenshot') uploadMutation.mutate()
    else manualMutation.mutate()
  }

  const resetAll = () => {
    setPhase('upload')
    setResult(null)
    setFiles([])
    setRegNumber('')
    setDriverName('')
    setPhoneNumber('')
  }

  const stepperStep = phase === 'upload' ? 1 : phase === 'confirm' ? 2 : 3

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-foreground">Verify a Driver</h1>
        <p className="text-muted-foreground mt-1">Check a driver's safety record before your ride.</p>
      </div>

      {phase !== 'result' && <VerifyStepper step={stepperStep} />}

      {phase === 'upload' && (
        <Card>
          <CardContent className="space-y-4 pt-6">
            <Tabs value={mode} onValueChange={(v) => setMode(v as VerifyMode)}>
              <TabsList className="w-full">
                <TabsTrigger value="screenshot" className="flex-1">
                  <Upload className="h-4 w-4" /> Screenshot
                </TabsTrigger>
                <TabsTrigger value="manual" className="flex-1">
                  <PenLine className="h-4 w-4" /> Manual Entry
                </TabsTrigger>
              </TabsList>
              <TabsContent value="screenshot" className="space-y-4 pt-4">
                <CardDescription>
                  Upload up to 3 screenshots (driver profile, vehicle, or details). Supported: JPG, PNG, WEBP — max 10MB each.
                </CardDescription>
                <ScreenshotDropzone files={files} onAddFiles={addFiles} onRemoveFile={removeFile} />
              </TabsContent>
              <TabsContent value="manual" className="space-y-4 pt-4">
                <CardDescription>
                  Enter the driver's details from your ride-hailing app. Registration number gives the most accurate result.
                </CardDescription>
                <ManualLookupForm
                  regNumber={regNumber}
                  driverName={driverName}
                  phoneNumber={phoneNumber}
                  onRegNumberChange={setRegNumber}
                  onDriverNameChange={setDriverName}
                  onPhoneNumberChange={setPhoneNumber}
                />
              </TabsContent>
            </Tabs>
            <Button className="w-full" disabled={!canContinue} onClick={() => setPhase('confirm')}>
              Continue
            </Button>
          </CardContent>
        </Card>
      )}

      {phase === 'confirm' && (
        <Card>
          <CardContent className="space-y-4 pt-6">
            <CardDescription>Check these details before we search community reports.</CardDescription>
            {mode === 'screenshot' ? (
              <ScreenshotThumbnails files={files} />
            ) : (
              <dl className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <dt className="text-muted-foreground">Registration</dt>
                  <dd className="font-medium text-foreground">{regNumber || '—'}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-muted-foreground">Driver name</dt>
                  <dd className="font-medium text-foreground">{driverName || '—'}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-muted-foreground">Phone</dt>
                  <dd className="font-medium text-foreground">{phoneNumber || '—'}</dd>
                </div>
              </dl>
            )}
            <div className="flex gap-2">
              <Button variant="outline" className="flex-1" onClick={() => setPhase('upload')}>
                Back
              </Button>
              <Button className="flex-1" onClick={submitVerification}>
                Confirm & Verify
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {phase === 'scanning' && <VerificationScanning />}

      {phase === 'result' && result && (
        <div className="space-y-4">
          <VerificationResultPanel result={result} />

          {canFollow && result.driverId && <FollowDriverButton driverId={result.driverId} />}

          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={resetAll}>
              Verify Another Driver
            </Button>
            {isPassenger && (
              <Button asChild variant="destructive" className="flex-1">
                <Link to="/passenger/report" state={{ driverName: result.driverName, registrationNumber: result.registrationNumber }}>
                  <Flag className="h-4 w-4 mr-2" /> Report Driver
                </Link>
              </Button>
            )}
            {!user && (
              <Button asChild variant="destructive" className="flex-1">
                <Link
                  to="/login"
                  state={{
                    from: '/passenger/report',
                    prefill: { driverName: result.driverName, registrationNumber: result.registrationNumber },
                  }}
                >
                  <Flag className="h-4 w-4 mr-2" /> Log in to Report
                </Link>
              </Button>
            )}
          </div>
          {!user && (
            <p className="text-center text-xs text-muted-foreground">
              Reporting a driver requires an account so we can keep reports accountable.
            </p>
          )}
        </div>
      )}
    </div>
  )
}
