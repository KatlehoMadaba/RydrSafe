import { Outlet, Link } from 'react-router-dom'
import { Shield } from 'lucide-react'

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 flex flex-col">
      <header className="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800 px-6 py-4 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2 w-fit">
          <Shield className="h-6 w-6 text-blue-600" />
          <span className="text-xl font-bold text-gray-900 dark:text-white">RydrSafe</span>
        </Link>
        <nav className="flex items-center gap-4 text-sm font-medium">
          <Link to="/verify" className="text-gray-600 dark:text-gray-300 hover:text-blue-600">
            Verify a Driver
          </Link>
          <Link to="/login" className="text-gray-600 dark:text-gray-300 hover:text-blue-600">
            Sign in
          </Link>
        </nav>
      </header>
      <main className="flex-1 flex items-center justify-center p-6">
        <Outlet />
      </main>
      <footer className="text-center text-sm text-gray-500 py-4">
        © {new Date().getFullYear()} RydrSafe. All rights reserved.
      </footer>
    </div>
  )
}
