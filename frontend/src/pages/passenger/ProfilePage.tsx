import { Link } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { User, Mail, Shield, History, ChevronRight } from 'lucide-react'

export function ProfilePage() {
  const { user } = useAuth()

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
        <CardContent className="space-y-4">
          <div className="flex items-center gap-3 py-3 border-b border-border">
            <User className="h-4 w-4 text-subtle" />
            <div>
              <p className="text-xs text-muted-foreground">Full Name</p>
              <p className="text-sm font-medium text-foreground">{user?.fullName}</p>
            </div>
          </div>
          <div className="flex items-center gap-3 py-3 border-b border-border">
            <Mail className="h-4 w-4 text-subtle" />
            <div>
              <p className="text-xs text-muted-foreground">Email</p>
              <p className="text-sm font-medium text-foreground">{user?.email}</p>
            </div>
          </div>
          <div className="flex items-center gap-3 py-3">
            <Shield className="h-4 w-4 text-subtle" />
            <div>
              <p className="text-xs text-muted-foreground">Member Since</p>
              <p className="text-sm font-medium text-foreground">
                {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <Link to="/passenger/history" className="flex items-center gap-3 p-4">
          <History className="h-4 w-4 text-subtle" />
          <span className="flex-1 text-sm font-medium text-foreground">Verification history</span>
          <ChevronRight className="h-4 w-4 text-subtle" />
        </Link>
      </Card>
    </div>
  )
}
