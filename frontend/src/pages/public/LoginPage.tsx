import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Logo } from '@/components/Logo'

const schema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
})
type FormData = z.infer<typeof schema>

export function LoginPage() {
  const { login, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const redirect = location.state as { from?: string; prefill?: unknown } | null

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  useEffect(() => {
    if (user) {
      // Return the passenger to the action they came from (e.g. reporting a driver), if any.
      if (redirect?.from && user.role === 'passenger') {
        navigate(redirect.from, { replace: true, state: redirect.prefill })
        return
      }
      const redirects: Record<string, string> = {
        passenger: '/passenger/dashboard',
        moderator: '/moderator/dashboard',
        admin: '/admin/dashboard',
      }
      navigate(redirects[user.role] ?? '/passenger/dashboard', { replace: true })
    }
  }, [user, navigate, redirect])

  const onSubmit = async (data: FormData) => {
    try {
      await login(data.email, data.password)
      toast.success('Welcome back!')
    } catch {
      toast.error('Invalid email or password')
    }
  }

  return (
    <Card className="w-full max-w-md">
      <CardHeader className="text-center">
        <div className="flex justify-center mb-2">
          <Logo size="xl" variant="dp" />
        </div>
        <CardTitle className="font-display text-2xl">Sign in to RydrSafe</CardTitle>
        <CardDescription>Verify drivers before your next ride</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="you@example.com" {...register('email')} />
            {errors.email && <p className="text-xs text-highrisk">{errors.email.message}</p>}
          </div>
          <div className="space-y-1">
            <Label htmlFor="password">Password</Label>
            <Input id="password" type="password" placeholder="••••••••" {...register('password')} />
            {errors.password && <p className="text-xs text-highrisk">{errors.password.message}</p>}
          </div>
          <Button type="submit" className="w-full" isLoading={isSubmitting}>
            Sign in
          </Button>
        </form>
      </CardContent>
      <CardFooter className="flex-col gap-3">
        <p className="text-sm text-muted-foreground">
          Don't have an account?{' '}
          <Link to="/register" className="text-teal-600 hover:underline font-medium">
            Sign up
          </Link>
        </p>
        <p className="text-sm text-muted-foreground">
          Just want to check a driver?{' '}
          <Link to="/verify" className="text-teal-600 hover:underline font-medium">
            Verify without an account
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
