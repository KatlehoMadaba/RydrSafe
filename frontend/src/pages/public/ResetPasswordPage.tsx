import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'
import { authApi } from '@/api/auth'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { PasswordInput } from '@/components/ui/password-input'
import { Logo } from '@/components/Logo'
import {
  Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle,
} from '@/components/ui/card'

const schema = z
  .object({
    newPassword: z.string().min(8, 'Password must be at least 8 characters'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  })
type FormData = z.infer<typeof schema>

export function ResetPasswordPage() {
  const [params] = useSearchParams()
  const navigate = useNavigate()
  const token = params.get('token')

  const {
    register, handleSubmit, formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    if (!token) return
    try {
      await authApi.resetPassword({ token, newPassword: data.newPassword })
      toast.success('Password updated. Sign in with your new password.')
      navigate('/login', { replace: true })
    } catch {
      // The server returns one error for unknown, expired and already-used tokens, so there is
      // nothing more specific to say — and guessing would be wrong as often as it was right.
      toast.error('This reset link is invalid or has expired. Request a new one.')
    }
  }

  // A link that arrived mangled, or someone opening the page directly, gets a way forward
  // rather than a form that cannot possibly succeed.
  if (!token) {
    return (
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="font-display text-2xl">Reset link incomplete</CardTitle>
          <CardDescription>
            This page needs the link from your reset email. Copy the whole link, or request a
            new one.
          </CardDescription>
        </CardHeader>
        <CardFooter>
          <Button asChild className="w-full">
            <Link to="/forgot-password">Request a new link</Link>
          </Button>
        </CardFooter>
      </Card>
    )
  }

  return (
    <Card className="w-full max-w-md">
      <CardHeader className="text-center">
        <div className="mb-2 flex justify-center">
          <Logo size="xl" variant="dp" />
        </div>
        <CardTitle className="font-display text-2xl">Set a new password</CardTitle>
        <CardDescription>
          Choose a new password. Signing in elsewhere will stop working.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="newPassword">New password</Label>
            <PasswordInput id="newPassword" placeholder="••••••••" {...register('newPassword')} />
            {errors.newPassword && (
              <p className="text-xs text-highrisk">{errors.newPassword.message}</p>
            )}
          </div>
          <div className="space-y-1">
            <Label htmlFor="confirmPassword">Confirm new password</Label>
            <PasswordInput
              id="confirmPassword" placeholder="••••••••" {...register('confirmPassword')}
            />
            {errors.confirmPassword && (
              <p className="text-xs text-highrisk">{errors.confirmPassword.message}</p>
            )}
          </div>
          <Button type="submit" className="w-full" isLoading={isSubmitting}>
            Update password
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}
