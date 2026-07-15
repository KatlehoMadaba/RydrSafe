import { Component, type ReactNode } from 'react'
import { ShieldAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
}

// A crash anywhere in the tree previously produced a blank white page — no
// safety product should fail silent. This is the last-resort fallback; page
// content should still handle its own loading/error/empty states.
export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  componentDidCatch(error: unknown, info: unknown) {
    console.error('Unhandled error in RydrSafe UI:', error, info)
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-background px-6 text-center">
          <ShieldAlert className="h-10 w-10 text-subtle" />
          <div className="space-y-1">
            <p className="font-display text-lg font-bold text-foreground">Something went wrong</p>
            <p className="text-sm text-muted-foreground">Please refresh the page. If this keeps happening, let us know.</p>
          </div>
          <Button variant="outline" onClick={() => window.location.reload()}>
            Refresh
          </Button>
        </div>
      )
    }

    return this.props.children
  }
}
