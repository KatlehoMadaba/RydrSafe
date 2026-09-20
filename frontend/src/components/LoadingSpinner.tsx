import { cn } from '@/lib/utils'

export function LoadingSpinner({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center justify-center', className)} role="status">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-secondary border-t-teal-500" />
      <span className="sr-only">Loading</span>
    </div>
  )
}
