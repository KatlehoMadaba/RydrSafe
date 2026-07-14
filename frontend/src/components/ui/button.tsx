import * as React from 'react'
import { Slot } from '@radix-ui/react-slot'
import { Loader2 } from 'lucide-react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground shadow-xs hover:bg-navy-800',
        destructive: 'bg-destructive text-destructive-foreground shadow-xs hover:bg-highrisk-strong',
        outline: 'border border-border bg-card text-foreground shadow-xs hover:bg-accent hover:text-accent-foreground',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-accent',
        soft: 'bg-navy-50 text-navy-700 hover:bg-navy-100',
        ghost: 'text-foreground hover:bg-accent hover:text-accent-foreground',
        link: 'text-teal-600 underline-offset-4 hover:text-teal-700 hover:underline',
      },
      // Default sizes clear the 44px WCAG 2.5.5 target. `compact` trades that for
      // density on moderator/admin surfaces, staying above the 24px AA floor.
      size: {
        default: 'h-11 px-5 py-2 compact:h-9 compact:px-4',
        sm: 'h-9 rounded-md px-3 text-xs compact:h-8',
        lg: 'h-12 rounded-lg px-8 text-base compact:h-10',
        icon: 'h-11 w-11 compact:h-9 compact:w-9',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  }
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
  isLoading?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, isLoading = false, disabled, children, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button'
    // Slot forwards to a single child, so the spinner can only be injected into a
    // real button. asChild callers (links) keep their own content untouched.
    const showSpinner = isLoading && !asChild

    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        aria-busy={isLoading || undefined}
        {...(asChild ? {} : { disabled: disabled || isLoading })}
        {...props}
      >
        {showSpinner ? (
          <>
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
            {children}
          </>
        ) : (
          children
        )}
      </Comp>
    )
  }
)
Button.displayName = 'Button'

export { Button, buttonVariants }
