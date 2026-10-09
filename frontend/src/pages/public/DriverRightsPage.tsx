import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { driverRightsApi } from '@/api/driverRights'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Badge } from '@/components/ui/badge'
import {
  Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { ShieldQuestion } from 'lucide-react'
import type { DriverSelfCheckResult } from '@/types'

const GROUNDS = [
  { value: 'Inaccuracy', label: 'The information held is inaccurate' },
  { value: 'MistakenIdentity', label: 'This is not me — mistaken identity' },
  { value: 'DuplicateProfile', label: 'There is a duplicate profile' },
  { value: 'ObjectionToProcessing', label: 'I object to this processing' },
  { value: 'Other', label: 'Other' },
]

/**
 * Part C. Drivers are not RydrSafe users, so this page is public. Everything on it is rate
 * limited server-side and the self-check answers identically whether or not a record exists —
 * otherwise it would be a way to test which registration numbers we hold.
 */
export function DriverRightsPage() {
  const [tab, setTab] = useState<'check' | 'appeal'>('check')

  return (
    <div className="mx-auto w-full max-w-2xl space-y-6">
      <div className="text-center">
        <ShieldQuestion className="mx-auto h-10 w-10 text-blue-600" />
        <h1 className="mt-2 text-2xl font-bold text-gray-900 dark:text-white">
          Driver rights
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          If you drive for an e-hailing service, you can ask what RydrSafe holds about you,
          correct it, or appeal a decision. You do not need an account.
        </p>
      </div>

      <div className="flex gap-2">
        <Button variant={tab === 'check' ? 'default' : 'outline'} onClick={() => setTab('check')}>
          Check my record
        </Button>
        <Button variant={tab === 'appeal' ? 'default' : 'outline'} onClick={() => setTab('appeal')}>
          Lodge an appeal
        </Button>
      </div>

      {tab === 'check' ? <SelfCheckForm /> : <AppealForm />}
    </div>
  )
}

function SelfCheckForm() {
  const [registrationNumber, setRegistrationNumber] = useState('')
  const [contactEmail, setContactEmail] = useState('')
  const [result, setResult] = useState<DriverSelfCheckResult | null>(null)

  const check = useMutation({
    mutationFn: () => driverRightsApi.selfCheck({ registrationNumber, contactEmail }),
    onSuccess: setResult,
    onError: (error) => {
      const message =
        (error as { response?: { data?: { error?: string } } })?.response?.data?.error
        ?? 'We could not complete that lookup.'
      toast.error(message)
    },
  })

  return (
    <Card>
      <CardHeader>
        <CardTitle>What do you hold about me?</CardTitle>
        <CardDescription>
          A POPIA section 23 request. We ask for a contact address so the lookup is attributable
          and so we can send you a copy.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1">
          <Label htmlFor="reg">Vehicle registration number</Label>
          <Input
            id="reg" placeholder="CA 123 456" value={registrationNumber}
            onChange={(e) => setRegistrationNumber(e.target.value)}
          />
        </div>
        <div className="space-y-1">
          <Label htmlFor="email">Your email address</Label>
          <Input
            id="email" type="email" placeholder="you@example.com" value={contactEmail}
            onChange={(e) => setContactEmail(e.target.value)}
          />
        </div>

        <Button
          className="w-full"
          disabled={!registrationNumber.trim() || !contactEmail.trim() || check.isPending}
          onClick={() => check.mutate()}
        >
          {check.isPending ? 'Checking…' : 'Check my record'}
        </Button>

        {result && (
          <div className="space-y-3 rounded-md border border-gray-200 p-4">
            <p className="text-sm text-gray-700">{result.message}</p>

            {result.recordExists && (
              <>
                <div className="flex flex-wrap items-center gap-2">
                  <span className="font-medium">{result.driverName}</span>
                  <Badge variant="secondary">{result.publicStatus}</Badge>
                  <span className="text-xs text-gray-500">
                    {result.corroboratedReportCount} corroborated report
                    {result.corroboratedReportCount === 1 ? '' : 's'}
                  </span>
                </div>

                {result.summaries.length === 0 ? (
                  <p className="text-xs text-gray-500">
                    Nothing about you is currently visible to other users.
                  </p>
                ) : (
                  <ul className="space-y-1 text-xs text-gray-600">
                    {result.summaries.map((s) => (
                      <li key={s.category}>
                        {s.category.replace(/([A-Z])/g, ' $1').trim()} · {s.severityBand} ·{' '}
                        {s.corroboratedCount}×
                        {s.earliestIncident && (
                          <> · {new Date(s.earliestIncident).toLocaleDateString()}
                            {s.latestIncident && s.latestIncident !== s.earliestIncident && (
                              <>–{new Date(s.latestIncident).toLocaleDateString()}</>
                            )}
                          </>
                        )}
                      </li>
                    ))}
                  </ul>
                )}
              </>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function AppealForm() {
  const [form, setForm] = useState({
    registrationNumber: '',
    grounds: 'Inaccuracy',
    detail: '',
    contactEmail: '',
    contactPhone: '',
    identityEvidenceNote: '',
  })

  const set = (key: keyof typeof form) => (value: string) =>
    setForm((prev) => ({ ...prev, [key]: value }))

  const lodge = useMutation({
    mutationFn: () => driverRightsApi.createAppeal(form),
    onSuccess: (data) => toast.success(data.message),
    onError: (error) => {
      const message =
        (error as { response?: { data?: { error?: string } } })?.response?.data?.error
        ?? 'We could not lodge that appeal.'
      toast.error(message)
    },
  })

  const incomplete =
    !form.registrationNumber.trim()
    || !form.detail.trim()
    || !form.contactEmail.trim()
    || !form.identityEvidenceNote.trim()

  return (
    <Card>
      <CardHeader>
        <CardTitle>Lodge an appeal</CardTitle>
        <CardDescription>
          While your appeal is open, your public status on RydrSafe is suspended. We respond
          within 30 days, and the moderator who made the decision under appeal will not be the
          one reviewing it.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1">
          <Label htmlFor="a-reg">Vehicle registration number</Label>
          <Input
            id="a-reg" value={form.registrationNumber}
            onChange={(e) => set('registrationNumber')(e.target.value)}
          />
        </div>

        <div className="space-y-1">
          <Label htmlFor="a-grounds">Grounds</Label>
          <Select value={form.grounds} onValueChange={set('grounds')}>
            <SelectTrigger id="a-grounds"><SelectValue /></SelectTrigger>
            <SelectContent>
              {GROUNDS.map((g) => (
                <SelectItem key={g.value} value={g.value}>{g.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1">
          <Label htmlFor="a-detail">What is wrong, and what should it say instead?</Label>
          <Textarea
            id="a-detail" rows={4} value={form.detail}
            onChange={(e) => set('detail')(e.target.value)}
          />
        </div>

        <div className="space-y-1">
          <Label htmlFor="a-evidence">How can you show you are this driver?</Label>
          <Textarea
            id="a-evidence" rows={2}
            placeholder="For example: I can provide a copy of the vehicle licence disc or my PrDP."
            value={form.identityEvidenceNote}
            onChange={(e) => set('identityEvidenceNote')(e.target.value)}
          />
          <p className="text-xs text-gray-500">
            Do not upload documents here. A moderator will ask for what they need.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-1">
            <Label htmlFor="a-email">Email</Label>
            <Input
              id="a-email" type="email" value={form.contactEmail}
              onChange={(e) => set('contactEmail')(e.target.value)}
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="a-phone">Phone (optional)</Label>
            <Input
              id="a-phone" value={form.contactPhone}
              onChange={(e) => set('contactPhone')(e.target.value)}
            />
          </div>
        </div>

        <Button className="w-full" disabled={incomplete || lodge.isPending} onClick={() => lodge.mutate()}>
          {lodge.isPending ? 'Sending…' : 'Lodge appeal'}
        </Button>
      </CardContent>
    </Card>
  )
}
