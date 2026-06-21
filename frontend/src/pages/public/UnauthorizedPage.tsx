import { Link } from 'react-router-dom'
import { ShieldAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function UnauthorizedPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-4 text-center p-6">
      <ShieldAlert className="h-16 w-16 text-red-500" />
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Access Denied</h1>
      <p className="text-gray-500 max-w-sm">You don't have permission to view this page.</p>
      <Button asChild><Link to="/">Go Home</Link></Button>
    </div>
  )
}
