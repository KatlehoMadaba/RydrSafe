import {
  LayoutDashboard,
  Search,
  Flag,
  Bell,
  User,
  FileText,
  Car,
  Users,
  UserCheck,
  BarChart2,
  type LucideIcon,
} from 'lucide-react'
import type { UserRole } from '@/types'

export interface NavItem {
  to: string
  icon: LucideIcon
  label: string
}

export interface RoleNavConfig {
  label: string
  nav: NavItem[]
  /** Passenger gets a bottom tab bar on mobile; moderator/admin get a drawer — density tooling doesn't fit a thumb-reach bar. */
  mobile: 'bottom' | 'drawer'
  density: 'comfortable' | 'compact'
  /** Only passenger has a standalone alerts destination outside its primary nav list. */
  showTopbarBell: boolean
}

// Role personality becomes mechanical here: AppShell stamps `density` as
// data-density on <main>, activating the `compact:` variants every primitive
// already declares. Passenger stays comfortable; moderator/admin are compact.
export const NAV_CONFIG: Record<UserRole, RoleNavConfig> = {
  passenger: {
    label: 'Passenger',
    density: 'comfortable',
    mobile: 'bottom',
    showTopbarBell: true,
    nav: [
      { to: '/passenger/dashboard', icon: LayoutDashboard, label: 'Home' },
      { to: '/passenger/verify', icon: Search, label: 'Verify' },
      { to: '/passenger/report', icon: Flag, label: 'Report' },
      { to: '/passenger/alerts', icon: Bell, label: 'Alerts' },
      { to: '/passenger/profile', icon: User, label: 'Profile' },
    ],
  },
  moderator: {
    label: 'Moderator',
    density: 'compact',
    mobile: 'drawer',
    showTopbarBell: false,
    nav: [
      { to: '/moderator/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
      { to: '/moderator/reports', icon: FileText, label: 'Reports' },
      { to: '/moderator/drivers', icon: Car, label: 'Drivers' },
      { to: '/moderator/notifications', icon: Bell, label: 'Notifications' },
    ],
  },
  admin: {
    label: 'Admin',
    density: 'compact',
    mobile: 'drawer',
    showTopbarBell: false,
    nav: [
      { to: '/admin/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
      { to: '/admin/users', icon: Users, label: 'Users' },
      { to: '/admin/moderators', icon: UserCheck, label: 'Moderators' },
      { to: '/admin/analytics', icon: BarChart2, label: 'Analytics' },
    ],
  },
}
