import { Outlet, Link } from 'react-router-dom'
import { Shield } from 'lucide-react'

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 flex flex-col">
      <header className="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800 px-6 py-4">
        <Link to="/" className="flex items-center gap-2 w-fit">
          <Shield className="h-6 w-6 text-blue-600" />
          <span className="text-xl font-bold text-gray-900 dark:text-white">RydrSafe</span>
        </Link>
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
