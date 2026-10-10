import {
  LayoutDashboard,
  Search,
  Bell,
  User,
  Lightbulb,
  FileText,
  Car,
  Gavel,
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
  /**
   * What the passenger bottom bar shows instead of {@link label}. The bar gives each entry
   * flex-1, so anything longer than "Alerts" truncates on a 360px screen — a destination whose
   * real name doesn't fit gets a short form here rather than a shortened name everywhere.
   */
  shortLabel?: string
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
      // Was a 'Report' shortcut straight to the submission form. Community Reports is where a
      // passenger actually starts — read what others reported, then report from the driver's
      // own card. The form is still reachable at /passenger/report, just no longer the entry.
      {
        to: '/passenger/community-reports',
        icon: Users,
        label: 'Community Reports',
        shortLabel: 'Reports',
      },
      { to: '/passenger/alerts', icon: Bell, label: 'Alerts' },
      // Sixth item: the bottom bar gives each entry flex-1, so this narrows the others
      // rather than overflowing. Keep the label short — anything longer than "Alerts"
      // starts truncating on a 360px screen.
      { to: '/passenger/recommendations', icon: Lightbulb, label: 'Ideas' },
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
      // Part C. Drivers can contest a flag, and someone has to see the queue.
      { to: '/moderator/appeals', icon: Gavel, label: 'Appeals' },
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
