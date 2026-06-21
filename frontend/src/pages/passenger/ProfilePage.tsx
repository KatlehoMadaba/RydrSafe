import { useAuth } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { User, Mail, Shield } from 'lucide-react'

export function ProfilePage() {
  const { user } = useAuth()

  return (
    <div className="max-w-lg mx-auto space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Profile</h1>
      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="h-16 w-16 rounded-full bg-blue-600 flex items-center justify-center text-white text-2xl font-bold">
              {user?.fullName?.[0]?.toUpperCase()}
            </div>
            <div>
              <CardTitle>{user?.fullName}</CardTitle>
              <Badge variant="default" className="mt-1 capitalize">{user?.role}</Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center gap-3 py-3 border-b border-gray-100 dark:border-gray-800">
            <User className="h-4 w-4 text-gray-400" />
            <div>
              <p className="text-xs text-gray-500">Full Name</p>
              <p className="text-sm font-medium text-gray-900 dark:text-white">{user?.fullName}</p>
            </div>
          </div>
          <div className="flex items-center gap-3 py-3 border-b border-gray-100 dark:border-gray-800">
            <Mail className="h-4 w-4 text-gray-400" />
            <div>
              <p className="text-xs text-gray-500">Email</p>
              <p className="text-sm font-medium text-gray-900 dark:text-white">{user?.email}</p>
            </div>
          </div>
          <div className="flex items-center gap-3 py-3">
            <Shield className="h-4 w-4 text-gray-400" />
            <div>
              <p className="text-xs text-gray-500">Member Since</p>
              <p className="text-sm font-medium text-gray-900 dark:text-white">
                {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
