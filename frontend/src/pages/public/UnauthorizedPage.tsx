import { Link } from 'react-router-dom'
import { ShieldAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function UnauthorizedPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-4 text-center p-6">
      <ShieldAlert className="h-16 w-16 text-highrisk" />
      <h1 className="font-display text-2xl font-bold text-foreground">Access Denied</h1>
      <p className="text-muted-foreground max-w-sm">You don't have permission to view this page.</p>
      <Button asChild><Link to="/">Go Home</Link></Button>
    </div>
  )
}
