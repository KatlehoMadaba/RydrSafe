import { Outlet, Link } from 'react-router-dom'
import { MovingBorderButton } from '@/components/ui/moving-border'
import { Logo } from '@/components/Logo'

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-background flex flex-col">
      <header className="bg-card border-b border-border px-4 sm:px-6 py-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
        <Link to="/" className="flex items-center">
          <Logo size="lg" />
        </Link>
        <nav className="flex items-center gap-4 text-sm font-medium">
          <MovingBorderButton as={Link} to="/verify">
            Verify a Driver
          </MovingBorderButton>
          <Link to="/login" className="text-muted-foreground hover:text-foreground">
            Sign in
          </Link>
        </nav>
      </header>
      <main className="flex-1 flex items-center justify-center p-6">
        <Outlet />
      </main>
      <footer className="text-center text-sm text-muted-foreground py-4">
        © {new Date().getFullYear()} RydrSafe. All rights reserved.
      </footer>
    </div>
  )
}
