import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { isAxiosError } from 'axios'
import { toast } from 'sonner'
import { useAuth } from '@/hooks/useAuth'
import { authApi } from '@/api/auth'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/ui/password-input'
import { Label } from '@/components/ui/label'
import {
  Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from '@/components/ui/dialog'
import { History, ChevronRight, LogOut, Shield, TriangleAlert } from 'lucide-react'

const detailsSchema = z.object({
  fullName: z.string().min(2, 'Full name must be at least 2 characters').max(255),
  email: z.string().email('Enter a valid email address'),
})
type DetailsForm = z.infer<typeof detailsSchema>

const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Enter your current password'),
    newPassword: z.string().min(8, 'Your new password must be at least 8 characters'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  })
  .refine((d) => d.newPassword !== d.currentPassword, {
    message: 'Your new password must be different from your current one',
    path: ['newPassword'],
  })
type PasswordForm = z.infer<typeof passwordSchema>

/** Typed verbatim before the delete button unlocks. A dialog alone is too easy to click through. */
const DELETE_PHRASE = 'DELETE'

/** ExceptionMiddleware serialises every handled failure as `{ error }`. */
const serverMessage = (err: unknown, fallback: string) =>
  (isAxiosError<{ error?: string }>(err) ? err.response?.data?.error : undefined) ?? fallback

export function ProfilePage() {
  const { user, logout, applyProfile, applySession } = useAuth()
  const navigate = useNavigate()

  const [deleteOpen, setDeleteOpen] = useState(false)
  const [deletePassword, setDeletePassword] = useState('')
  const [deleteReason, setDeleteReason] = useState('')
  const [deleteConfirm, setDeleteConfirm] = useState('')
  const [deleting, setDeleting] = useState(false)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const details = useForm<DetailsForm>({
    resolver: zodResolver(detailsSchema),
    values: { fullName: user?.fullName ?? '', email: user?.email ?? '' },
  })

  const password = useForm<PasswordForm>({ resolver: zodResolver(passwordSchema) })

  // The session is gone the moment deletion succeeds, so nothing should render off it.
  useEffect(() => {
    if (!user) navigate('/login', { replace: true })
  }, [user, navigate])

  const onSaveDetails = async (data: DetailsForm) => {
    try {
      const updated = await authApi.updateProfile(data)
      applyProfile({ fullName: updated.fullName, email: updated.email })
      details.reset(data)
      toast.success('Your details have been updated.')
    } catch (err) {
      const message = serverMessage(err, 'Could not save your details. Please try again.')
      // An address another account already holds belongs next to the field, not in a toast.
      if (message.toLowerCase().includes('already registered')) {
        details.setError('email', { message })
        return
      }
      toast.error(message)
    }
  }

  const onChangePassword = async (data: PasswordForm) => {
    try {
      const session = await authApi.changePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      })
      // The server rotated the refresh token, so adopt the new pair or this tab is next to go.
      applySession(session)
      password.reset()
      toast.success('Password changed. Any other devices have been signed out.')
    } catch (err) {
      // Put the server's complaint next to the field it is about. These come back as 400 so a
      // mistyped password does not trip the 401 interceptor and sign the user out.
      const message = serverMessage(err, 'Could not change your password.')
      const lower = message.toLowerCase()

      if (lower.includes('current password is incorrect')) {
        password.setError('currentPassword', { message })
        return
      }
      if (lower.includes('new password')) {
        password.setError('newPassword', { message })
        return
      }
      toast.error(message)
    }
  }

  const onDelete = async () => {
    setDeleting(true)
    setDeleteError(null)
    try {
      const result = await authApi.deleteAccount({
        password: deletePassword,
        reason: deleteReason.trim() || undefined,
      })
      setDeleteOpen(false)
      logout()
      navigate('/login', { replace: true, state: { accountDeleted: result.message } })
    } catch (err) {
      setDeleteError(serverMessage(err, 'Could not delete your account. Please try again.'))
    } finally {
      setDeleting(false)
    }
  }

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  const canDelete = deletePassword.length > 0 && deleteConfirm === DELETE_PHRASE && !deleting

  return (
    <div className="max-w-lg mx-auto space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Profile</h1>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="h-16 w-16 rounded-full bg-primary flex items-center justify-center text-primary-foreground text-2xl font-bold">
              {user?.fullName?.[0]?.toUpperCase()}
            </div>
            <div>
              <CardTitle>{user?.fullName}</CardTitle>
              <Badge variant="default" className="mt-1 capitalize">
                {user?.role}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-3 py-2">
            <Shield className="h-4 w-4 text-subtle" />
            <div>
              <p className="text-xs text-muted-foreground">Member since</p>
              <p className="text-sm font-medium text-foreground">
                {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Your details</CardTitle>
          <CardDescription>
            You can correct what we hold about you at any time (clause 4, POPIA s24). Your email
            address is also how you sign in.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={details.handleSubmit(onSaveDetails)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="fullName">Full name</Label>
              <Input id="fullName" {...details.register('fullName')} />
              {details.formState.errors.fullName && (
                <p className="text-xs text-highrisk">{details.formState.errors.fullName.message}</p>
              )}
            </div>
            <div className="space-y-1">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" {...details.register('email')} />
              {details.formState.errors.email && (
                <p className="text-xs text-highrisk">{details.formState.errors.email.message}</p>
              )}
            </div>
            <Button
              type="submit"
              isLoading={details.formState.isSubmitting}
              disabled={!details.formState.isDirty}
            >
              Save changes
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Password</CardTitle>
          <CardDescription>Changing your password signs you out everywhere else.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={password.handleSubmit(onChangePassword)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="currentPassword">Current password</Label>
              <PasswordInput id="currentPassword" {...password.register('currentPassword')} />
              {password.formState.errors.currentPassword && (
                <p className="text-xs text-highrisk">
                  {password.formState.errors.currentPassword.message}
                </p>
              )}
            </div>
            <div className="space-y-1">
              <Label htmlFor="newPassword">New password</Label>
              <PasswordInput id="newPassword" {...password.register('newPassword')} />
              {password.formState.errors.newPassword && (
                <p className="text-xs text-highrisk">{password.formState.errors.newPassword.message}</p>
              )}
            </div>
            <div className="space-y-1">
              <Label htmlFor="confirmPassword">Confirm new password</Label>
              <PasswordInput id="confirmPassword" {...password.register('confirmPassword')} />
              {password.formState.errors.confirmPassword && (
                <p className="text-xs text-highrisk">
                  {password.formState.errors.confirmPassword.message}
                </p>
              )}
            </div>
            <Button type="submit" variant="outline" isLoading={password.formState.isSubmitting}>
              Change password
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <Link to="/passenger/history" className="flex items-center gap-3 p-4">
          <History className="h-4 w-4 text-subtle" />
          <span className="flex-1 text-sm font-medium text-foreground">Verification history</span>
          <ChevronRight className="h-4 w-4 text-subtle" />
        </Link>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <Button variant="outline" className="w-full justify-start" onClick={handleLogout}>
            <LogOut className="h-4 w-4" />
            Sign out
          </Button>
        </CardContent>
      </Card>

      <Card className="border-highrisk-muted">
        <CardHeader>
          <CardTitle className="text-base text-highrisk-strong">Delete your account</CardTitle>
          <CardDescription>This is immediate and cannot be undone.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert variant="highrisk">
            <TriangleAlert />
            <AlertTitle>What deletion does</AlertTitle>
            <AlertDescription>
              <ul className="list-disc space-y-1 pl-4">
                <li>
                  Your account, consents, notifications, followed drivers and verification history
                  are deleted.
                </li>
                <li>
                  Reports you submitted are <strong>kept but unlinked from you</strong>. They
                  describe other people, and a driver disputing one is entitled to have it
                  reviewed (clause 29.2).
                </li>
                <li>
                  Your email address is released, so you can sign up again with it whenever you
                  like.
                </li>
              </ul>
            </AlertDescription>
          </Alert>
          <Button variant="destructive" onClick={() => setDeleteOpen(true)}>
            Delete my account
          </Button>
        </CardContent>
      </Card>

      <Dialog open={deleteOpen} onOpenChange={(open) => !deleting && setDeleteOpen(open)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete your account?</DialogTitle>
            <DialogDescription>
              This happens straight away and there is no undo. Reports you submitted stay on
              record, unlinked from you.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="deletePassword">Your password</Label>
              <PasswordInput
                id="deletePassword"
                value={deletePassword}
                onChange={(e) => setDeletePassword(e.target.value)}
                autoComplete="current-password"
              />
            </div>
            <div className="space-y-1">
              <Label htmlFor="deleteConfirm">
                Type <span className="font-mono font-semibold">{DELETE_PHRASE}</span> to confirm
              </Label>
              <Input
                id="deleteConfirm"
                value={deleteConfirm}
                onChange={(e) => setDeleteConfirm(e.target.value)}
                autoComplete="off"
              />
            </div>
            <div className="space-y-1">
              <Label htmlFor="deleteReason">Why are you leaving? (optional)</Label>
              <Input
                id="deleteReason"
                value={deleteReason}
                onChange={(e) => setDeleteReason(e.target.value)}
                maxLength={500}
              />
            </div>
            {deleteError && <p className="text-xs text-highrisk">{deleteError}</p>}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteOpen(false)} disabled={deleting}>
              Keep my account
            </Button>
            <Button
              variant="destructive"
              onClick={onDelete}
              disabled={!canDelete}
              isLoading={deleting}
            >
              Delete permanently
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
