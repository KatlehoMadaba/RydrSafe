import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'sonner'
import { AuthProvider } from '@/hooks/useAuth'
import { ProtectedRoute } from '@/routes/ProtectedRoute'

import { PublicLayout } from '@/layouts/PublicLayout'
import { PassengerLayout } from '@/layouts/PassengerLayout'
import { ModeratorLayout } from '@/layouts/ModeratorLayout'
import { AdminLayout } from '@/layouts/AdminLayout'

import { LoginPage } from '@/pages/public/LoginPage'
import { RegisterPage } from '@/pages/public/RegisterPage'
import { UnauthorizedPage } from '@/pages/public/UnauthorizedPage'

import { PassengerDashboardPage } from '@/pages/passenger/DashboardPage'
import { VerifyDriverPage } from '@/pages/passenger/VerifyDriverPage'
import { ReportDriverPage } from '@/pages/passenger/ReportDriverPage'
import { HistoryPage } from '@/pages/passenger/HistoryPage'
import { ProfilePage } from '@/pages/passenger/ProfilePage'

import { ModeratorDashboardPage } from '@/pages/moderator/ModeratorDashboardPage'
import { ModeratorReportsPage } from '@/pages/moderator/ReportsPage'
import { ModeratorDriversPage } from '@/pages/moderator/DriversPage'
import { ModeratorNotificationsPage } from '@/pages/moderator/NotificationsPage'

import { AdminDashboardPage } from '@/pages/admin/AdminDashboardPage'
import { AdminUsersPage } from '@/pages/admin/UsersPage'
import { AdminModeratorsPage } from '@/pages/admin/ModeratorsPage'
import { AdminAnalyticsPage } from '@/pages/admin/AnalyticsPage'

const qc = new QueryClient({ defaultOptions: { queries: { retry: 1, staleTime: 30_000 } } })

export default function App() {
  return (
    <QueryClientProvider client={qc}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            {/* Public */}
            <Route element={<PublicLayout />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/unauthorized" element={<UnauthorizedPage />} />
            </Route>

            {/* Passenger */}
            <Route element={<ProtectedRoute allowedRoles={['passenger']} />}>
              <Route element={<PassengerLayout />}>
                <Route path="/passenger/dashboard" element={<PassengerDashboardPage />} />
                <Route path="/passenger/verify" element={<VerifyDriverPage />} />
                <Route path="/passenger/report" element={<ReportDriverPage />} />
                <Route path="/passenger/history" element={<HistoryPage />} />
                <Route path="/passenger/profile" element={<ProfilePage />} />
              </Route>
            </Route>

            {/* Moderator */}
            <Route element={<ProtectedRoute allowedRoles={['moderator']} />}>
              <Route element={<ModeratorLayout />}>
                <Route path="/moderator/dashboard" element={<ModeratorDashboardPage />} />
                <Route path="/moderator/reports" element={<ModeratorReportsPage />} />
                <Route path="/moderator/drivers" element={<ModeratorDriversPage />} />
                <Route path="/moderator/notifications" element={<ModeratorNotificationsPage />} />
              </Route>
            </Route>

            {/* Admin */}
            <Route element={<ProtectedRoute allowedRoles={['admin']} />}>
              <Route element={<AdminLayout />}>
                <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
                <Route path="/admin/users" element={<AdminUsersPage />} />
                <Route path="/admin/moderators" element={<AdminModeratorsPage />} />
                <Route path="/admin/analytics" element={<AdminAnalyticsPage />} />
              </Route>
            </Route>

            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </BrowserRouter>
        <Toaster position="top-right" richColors />
      </AuthProvider>
    </QueryClientProvider>
  )
}
