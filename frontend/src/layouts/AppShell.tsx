import { useState } from 'react'
import { Outlet, NavLink, Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { LogOut, Menu, Bell } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { notificationsApi } from '@/api/notifications'
import { Button } from '@/components/ui/button'
import { Logo } from '@/components/Logo'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet'
import { NAV_CONFIG, type NavItem } from './navConfig'
import { cn } from '@/lib/utils'
import type { UserRole } from '@/types'

/**
 * One shell for all three roles. Desktop always gets the w-64 sidebar; mobile
 * gets a passenger bottom bar or a moderator/admin drawer. `data-density`
 * activates the `compact:` variants every primitive already declares — this is
 * where "same system, different personality" becomes real.
 */
export function AppShell({ role }: { role: UserRole }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const config = NAV_CONFIG[role]

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const { data: notifications } = useQuery({
    queryKey: ['notifications'],
    queryFn: notificationsApi.getAll,
    enabled: config.showTopbarBell,
  })
  const unreadCount = notifications?.filter((n) => !n.isRead).length ?? 0

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    cn(
      'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      isActive ? 'bg-navy-50 text-navy-700' : 'text-muted-foreground hover:bg-accent hover:text-foreground'
    )

  const renderNavLinks = (items: NavItem[], onNavigate?: () => void) =>
    items.map(({ to, icon: Icon, label }) => (
      <NavLink key={to} to={to} onClick={onNavigate} className={navLinkClass}>
        <Icon className="h-4 w-4" />
        {label}
      </NavLink>
    ))

  return (
    <div data-density={config.density} className="min-h-screen bg-background md:flex">
      {/* Desktop sidebar */}
      <aside className="hidden w-64 shrink-0 flex-col border-r border-border bg-card md:flex">
        <div className="border-b border-border p-6">
          <Logo size="md" />
          <span className="mt-1 block text-xs font-semibold text-muted-foreground">{config.label}</span>
        </div>
        <nav className="flex-1 space-y-1 p-4">{renderNavLinks(config.nav)}</nav>
        <div className="border-t border-border p-4">
          <div className="mb-3 flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground">
              {user?.fullName?.[0]?.toUpperCase()}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-foreground">{user?.fullName}</p>
              <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            className="w-full justify-start text-destructive hover:bg-destructive/10 hover:text-destructive"
            onClick={handleLogout}
          >
            <LogOut className="h-4 w-4 mr-2" />
            Logout
          </Button>
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        {/* Topbar */}
        <header className="flex items-center justify-between gap-4 border-b border-border bg-card px-4 py-4 md:justify-end md:px-6">
          {config.mobile === 'drawer' ? (
            <Sheet open={drawerOpen} onOpenChange={setDrawerOpen}>
              <SheetTrigger asChild>
                <Button variant="ghost" size="icon" className="md:hidden" aria-label="Open menu">
                  <Menu className="h-5 w-5" />
                </Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-72 p-0">
                <SheetHeader className="border-b border-border">
                  <SheetTitle>
                    <Logo size="sm" />
                  </SheetTitle>
                </SheetHeader>
                <nav className="space-y-1 p-4">{renderNavLinks(config.nav, () => setDrawerOpen(false))}</nav>
              </SheetContent>
            </Sheet>
          ) : (
            <Link to={config.nav[0].to} className="flex items-center md:hidden">
              <Logo size="sm" />
            </Link>
          )}

          {config.showTopbarBell && (
            <Button variant="ghost" size="icon" className="relative" aria-label="Notifications" asChild>
              <Link to="/passenger/alerts">
                <Bell className="h-5 w-5" />
                {unreadCount > 0 && (
                  <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-highrisk" aria-hidden="true" />
                )}
              </Link>
            </Button>
          )}
        </header>

        <main className={cn('flex-1 overflow-auto p-4 md:p-6', config.mobile === 'bottom' && 'pb-24 md:pb-6')}>
          <Outlet />
        </main>

        {/* Passenger bottom nav */}
        {config.mobile === 'bottom' && (
          <nav
            className="fixed inset-x-0 bottom-0 z-40 flex h-16 items-center justify-around border-t border-border bg-card pb-[env(safe-area-inset-bottom)] md:hidden"
            aria-label="Primary"
          >
            {config.nav.map(({ to, icon: Icon, label }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  cn('flex flex-1 flex-col items-center gap-1 py-2 text-xs font-medium', isActive ? 'text-teal-600' : 'text-muted-foreground')
                }
              >
                <Icon className="h-5 w-5" />
                {label}
              </NavLink>
            ))}
          </nav>
        )}
      </div>
    </div>
  )
}
