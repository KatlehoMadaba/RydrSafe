import * as React from 'react'
import { Eye, EyeOff } from 'lucide-react'
import { Input, type InputProps } from '@/components/ui/input'
import { cn } from '@/lib/utils'

/**
 * A password field with a show/hide toggle.
 *
 * Typing a password blind is the main reason people mistype one and then cannot tell
 * whether the login failed because of the password or the account, so the toggle is a
 * correctness aid, not decoration.
 *
 * Forwards its ref so react-hook-form's `register()` spread works exactly as on `Input`.
 */
export const PasswordInput = React.forwardRef<HTMLInputElement, Omit<InputProps, 'type'>>(
  ({ className, ...props }, ref) => {
    const [visible, setVisible] = React.useState(false)

    return (
      <div className="relative">
        <Input
          {...props}
          ref={ref}
          type={visible ? 'text' : 'password'}
          // Room for the button so a long password never runs underneath it.
          className={cn('pr-10', className)}
        />
        <button
          type="button"
          onClick={() => setVisible((v) => !v)}
          // Revealing is a deliberate act, so the button stays out of the tab order:
          // tabbing through the form should move to the next field, not the toggle.
          tabIndex={-1}
          aria-label={visible ? 'Hide password' : 'Show password'}
          aria-pressed={visible}
          className="absolute inset-y-0 right-0 flex items-center px-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
        >
          {visible ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
        </button>
      </div>
    )
  }
)
PasswordInput.displayName = 'PasswordInput'
