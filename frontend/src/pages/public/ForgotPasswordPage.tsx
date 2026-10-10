import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link } from 'react-router-dom'
import { MailCheck } from 'lucide-react'
import { authApi } from '@/api/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Logo } from '@/components/Logo'
import {
  Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle,
} from '@/components/ui/card'

const schema = z.object({
  email: z.string().email('Enter the email address on your account'),
})
type FormData = z.infer<typeof schema>

export function ForgotPasswordPage() {
  const [submitted, setSubmitted] = useState(false)

  const {
    register, handleSubmit, formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    // Deliberately no try/catch branch that distinguishes outcomes: the confirmation is shown
    // whatever happens. Saying "no account with that email" here would let anyone test an
    // address list against RydrSafe and learn who has an account.
    try {
      await authApi.forgotPassword(data.email)
    } catch {
      // Swallowed for the same reason — a failure must not be distinguishable from a success.
    }
    setSubmitted(true)
  }

  if (submitted) {
    return (
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mb-2 flex justify-center">
            <MailCheck className="h-10 w-10 text-safe" />
          </div>
          <CardTitle className="font-display text-2xl">Check your email</CardTitle>
          <CardDescription>
            If that address has a RydrSafe account, we have sent it a link to reset the password.
            The link expires in one hour and can only be used once.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Nothing arrived? Check your spam folder, and make sure you used the address you signed
            up with.
          </p>
        </CardContent>
        <CardFooter>
          <Button asChild variant="outline" className="w-full">
            <Link to="/login">Back to sign in</Link>
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
        <CardTitle className="font-display text-2xl">Reset your password</CardTitle>
        <CardDescription>
          Enter your email address and we will send you a link to set a new password.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="you@example.com" {...register('email')} />
            {errors.email && <p className="text-xs text-highrisk">{errors.email.message}</p>}
          </div>
          <Button type="submit" className="w-full" isLoading={isSubmitting}>
            Send reset link
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          Remembered it?{' '}
          <Link to="/login" className="text-primary hover:underline">Sign in</Link>
        </p>
      </CardFooter>
    </Card>
  )
}
