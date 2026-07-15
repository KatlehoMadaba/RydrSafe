import { useRef, type ElementType, type ReactNode } from 'react'
import { motion, useAnimationFrame, useMotionTemplate, useMotionValue, useTransform } from 'framer-motion'
import { cn } from '@/lib/utils'

/**
 * A button with a light sweeping along its border — adapted from the 21st.dev
 * "moving border" pattern onto RydrSafe tokens. The original used a dark
 * slate background and a sky-blue glow behind a `dark:` strategy this app
 * doesn't use; this version runs on navy/teal instead, at the button primitive's
 * own size scale (h-11, rounded-md) rather than the demo's oversized pill.
 */

export interface MovingBorderButtonProps {
  borderRadius?: string
  children: ReactNode
  as?: ElementType
  containerClassName?: string
  borderClassName?: string
  duration?: number
  className?: string
  [key: string]: unknown
}

export function MovingBorderButton({
  borderRadius = 'var(--radius-md)',
  children,
  as: Component = 'button',
  containerClassName,
  borderClassName,
  duration = 3000,
  className,
  ...otherProps
}: MovingBorderButtonProps) {
  return (
    <Component
      className={cn('relative h-11 w-fit overflow-hidden bg-transparent p-px compact:h-9', containerClassName)}
      style={{ borderRadius }}
      {...otherProps}
    >
      <div className="absolute inset-0" style={{ borderRadius: `calc(${borderRadius} * 0.96)` }}>
        <MovingBorder duration={duration} rx="30%" ry="30%">
          <div className={cn('h-10 w-10 bg-[radial-gradient(var(--color-teal-500)_40%,transparent_60%)] opacity-70', borderClassName)} />
        </MovingBorder>
      </div>

      <div
        className={cn(
          'relative flex h-full w-full items-center justify-center gap-2 whitespace-nowrap px-5 text-sm font-medium text-primary-foreground antialiased',
          'bg-primary',
          className
        )}
        style={{ borderRadius: `calc(${borderRadius} * 0.96)` }}
      >
        {children}
      </div>
    </Component>
  )
}

export function MovingBorder({
  children,
  duration = 3000,
  rx,
  ry,
  ...otherProps
}: {
  children: ReactNode
  duration?: number
  rx?: string
  ry?: string
  [key: string]: unknown
}) {
  const pathRef = useRef<SVGRectElement>(null)
  const progress = useMotionValue<number>(0)

  useAnimationFrame((time) => {
    const length = pathRef.current?.getTotalLength()
    if (length) {
      const pxPerMillisecond = length / duration
      progress.set((time * pxPerMillisecond) % length)
    }
  })

  const x = useTransform(progress, (val) => pathRef.current?.getPointAtLength(val).x)
  const y = useTransform(progress, (val) => pathRef.current?.getPointAtLength(val).y)

  const transform = useMotionTemplate`translateX(${x}px) translateY(${y}px) translateX(-50%) translateY(-50%)`

  return (
    <>
      <svg xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="none" className="absolute h-full w-full" width="100%" height="100%" {...otherProps}>
        <rect fill="none" width="100%" height="100%" rx={rx} ry={ry} ref={pathRef} />
      </svg>
      <motion.div style={{ position: 'absolute', top: 0, left: 0, display: 'inline-block', transform }}>{children}</motion.div>
    </>
  )
}
