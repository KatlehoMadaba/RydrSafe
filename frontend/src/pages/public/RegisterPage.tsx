import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { platformApi } from '@/api/platform'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/ui/password-input'
import { Label } from '@/components/ui/label'
import {
  Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle,
} from '@/components/ui/card'
import { Logo } from '@/components/Logo'

/**
 * Part D of the user agreement. Each box is a separate, recorded acceptance — the server stores
 * one row per key with the agreement version, timestamp and IP, so a bundled "I agree to
 * everything" tick would not satisfy it.
 *
 * Keys must match `ConsentKeys` on the server exactly; changing one breaks the consent audit
 * trail rather than failing loudly.
 */
type Consent = { key: string; label: string; conspicuous?: boolean }

const CONSENTS: Consent[] = [
  {
    key: 'terms.parts-a-to-c',
    label: 'I have read and accept the Terms of Use, the Privacy Notice, and the Driver Rights sections (Parts A, B and C).',
  },
  {
    key: 'terms.clause-15-risk-limitation',
    label: 'I have read clause 15, which limits RydrSafe’s liability, and I understand it does not exclude liability for death, personal injury, gross negligence or fraud.',
    conspicuous: true,
  },
  {
    key: 'eligibility.age-18-plus',
    label: 'I confirm I am 18 years or older.',
  },
  {
    key: 'reporting.false-report-consequences',
    label: 'I understand that submitting a report I know to be false may lead to suspension and may expose me to a defamation claim.',
  },
  {
    key: 'privacy.screenshot-handling',
    label: 'I understand that screenshots I upload are used to extract text and are then discarded, and that only a hash is retained.',
  },
  {
    key: 'verification.risk-score-not-proof',
    label: 'I understand that a risk score reflects community reports and is not proof that a driver has committed any offence.',
  },
]

/**
 * react-hook-form reads a dot in a field name as a path separator, so registering
 * `terms.parts-a-to-c` builds `{ terms: { 'parts-a-to-c': true } }` rather than the flat key the
 * schema declares. The flat key then reads as `undefined`, `z.literal(true)` rejects it, and
 * submission fails silently with no error rendered anywhere — the button looks dead (issue #37).
 *
 * The consent keys themselves are the audit trail and must not change, so only the form field
 * name is rewritten. The key goes to the server exactly as declared in `CONSENTS`.
 */
const fieldName = (consentKey: string) => consentKey.replace(/\./g, '_')

const EIGHTEEN_YEARS_AGO = () => {
  const d = new Date()
  d.setFullYear(d.getFullYear() - 18)
  return d
}

const schema = z
  .object({
    fullName: z.string().min(2, 'Full name must be at least 2 characters'),
    email: z.string().email('Invalid email address'),
    password: z.string().min(8, 'Password must be at least 8 characters'),
    confirmPassword: z.string(),
    dateOfBirth: z
      .string()
      .min(1, 'Date of birth is required')
      .refine((v) => !Number.isNaN(Date.parse(v)), 'Enter a valid date')
      .refine((v) => new Date(v) <= EIGHTEEN_YEARS_AGO(), 'You must be 18 or older to use RydrSafe'),
    // Each consent is its own required boolean, so the user cannot proceed on a partial set.
    ...Object.fromEntries(
      CONSENTS.map((c) => [
        fieldName(c.key),
        z.literal(true, { message: 'This is required' }),
      ]),
    ),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  })

type FormData = z.infer<typeof schema>

export function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()

  // The agreement version is whatever the server is currently serving — the client records
  // consent against that, never against a version it hardcoded and may have drifted from.
  const { data: config } = useQuery({
    queryKey: ['platform-config'],
    queryFn: platformApi.getConfig,
    staleTime: 5 * 60_000,
  })

  const [emailTaken, setEmailTaken] = useState(false)

  const {
    register, handleSubmit, setError, clearErrors, formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    clearErrors('email')
    try {
      await registerUser({
        fullName: data.fullName,
        email: data.email,
        password: data.password,
        dateOfBirth: data.dateOfBirth,
        agreementVersion: config?.agreementVersion ?? '1.0',
        locale: 'en-ZA',
        consents: CONSENTS.map((c) => ({
          consentKey: c.key,
          accepted: Boolean((data as Record<string, unknown>)[fieldName(c.key)]),
        })),
      })
      toast.success('Account created. Welcome to RydrSafe.')
      navigate('/passenger/dashboard')
    } catch (error) {
      const message =
        (error as { response?: { data?: { error?: string } } })?.response?.data?.error
        ?? 'Registration failed. Please try again.'

      // "Email already registered" is a problem with one field, not the submission as a
      // whole. A toast disappears and leaves the form looking fine, so pin it to the email
      // input instead — the offer to sign in is rendered next to it below.
      if (/already registered/i.test(message)) {
        setEmailTaken(true)
        setError('email', { type: 'server', message: 'An account with this email already exists.' })
        return
      }

      setEmailTaken(false)
      toast.error(message)
    }
  }

  const fieldError = (key: string) =>
    (errors as Record<string, { message?: string } | undefined>)[key]?.message

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader className="text-center">
        <div className="flex justify-center mb-2">
          <Logo size="xl" variant="dp" />
        </div>
        <CardTitle className="font-display text-2xl">Create your account</CardTitle>
        <CardDescription>Join the community keeping riders safe</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="fullName">Full Name</Label>
            <Input id="fullName" placeholder="Jane Doe" {...register('fullName')} />
            {errors.fullName && <p className="text-xs text-highrisk">{errors.fullName.message}</p>}
          </div>

          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="you@example.com" {...register('email')} />
            <p className="text-xs text-muted-foreground">
              Would rather we never held your real address? A disposable one from a service such
              as{' '}
              <a
                href="https://temp-mail.org/en/"
                target="_blank"
                rel="noreferrer noopener"
                className="text-primary hover:underline"
              >
                temp-mail.org
              </a>{' '}
              works here. Lose access to that mailbox and you lose the account with it — there is
              no other way back in.
            </p>
            {errors.email && <p className="text-xs text-highrisk">{errors.email.message}</p>}
            {emailTaken && (
              <p className="text-xs text-muted-foreground">
                Already have an account?{' '}
                <Link to="/login" className="text-primary hover:underline">
                  Sign in instead
                </Link>
              </p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="dateOfBirth">Date of birth</Label>
            <Input id="dateOfBirth" type="date" {...register('dateOfBirth')} />
            <p className="text-xs text-muted-foreground">
              RydrSafe is for adults only. Reports here concern allegations of criminal conduct.
            </p>
            {errors.dateOfBirth && (
              <p className="text-xs text-highrisk">{errors.dateOfBirth.message}</p>
            )}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1">
              <Label htmlFor="password">Password</Label>
              <PasswordInput id="password" placeholder="••••••••" {...register('password')} />
              {errors.password && <p className="text-xs text-highrisk">{errors.password.message}</p>}
            </div>
            <div className="space-y-1">
              <Label htmlFor="confirmPassword">Confirm Password</Label>
              <PasswordInput
                id="confirmPassword" placeholder="••••••••"
                {...register('confirmPassword')}
              />
              {errors.confirmPassword && (
                <p className="text-xs text-highrisk">{errors.confirmPassword.message}</p>
              )}
            </div>
          </div>

          <div className="space-y-3 rounded-md border border-border p-4">
            <p className="text-sm font-medium text-foreground">Before you continue</p>
            <p className="text-xs text-muted-foreground">
              Read the{' '}
              <Link to="/legal/user-agreement" className="text-primary hover:underline">
                RydrSafe User Agreement
              </Link>
              . Each item below is recorded separately against your account.
            </p>

            {CONSENTS.map((consent) => (
              <div
                key={consent.key}
                // Clause 15 is called out visually because a liability limitation has to be
                // conspicuous to be enforceable — it cannot read like the other boxes.
                className={
                  consent.conspicuous
                    ? 'rounded border-2 border-review bg-review/10 p-3'
                    : undefined
                }
              >
                <label className="flex items-start gap-2 text-sm text-foreground">
                  <input
                    type="checkbox"
                    className="mt-1 h-4 w-4 shrink-0"
                    {...register(fieldName(consent.key) as keyof FormData)}
                  />
                  <span>{consent.label}</span>
                </label>
                {fieldError(fieldName(consent.key)) && (
                  <p className="ml-6 text-xs text-highrisk">
                    {fieldError(fieldName(consent.key))}
                  </p>
                )}
              </div>
            ))}
          </div>

          <Button type="submit" className="w-full" isLoading={isSubmitting}>
            Create account
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          Already have an account?{' '}
          <Link to="/login" className="text-teal-600 hover:underline font-medium">
            Sign in
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
