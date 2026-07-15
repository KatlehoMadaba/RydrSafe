import { cn } from '@/lib/utils'

export type LogoSize = 'sm' | 'md' | 'lg' | 'xl'
export type LogoVariant = 'full' | 'dp'

// One semantic scale, reused everywhere the logo appears — no file invents
// its own one-off height again. sm/md never share a screen with lg/xl, so
// there's no risk of a smaller tier reading as competing with a larger one.
const SIZE_CLASSES: Record<LogoSize, string> = {
  sm: 'h-12', // compact nav: mobile topbar, drawer headers — 48px
  md: 'h-14', // desktop sidebar (already gated by its parent's md:flex) — 56px
  lg: 'h-12 sm:h-14 md:h-16 lg:h-20', // public header — visible at every width, 48→56→64→80px
  xl: 'h-20 sm:h-24', // auth cards / future marketing — 80→96px
}

const VARIANT_ASSET: Record<LogoVariant, { src: string; width: number; height: number; rounded?: boolean }> = {
  full: { src: '/branding/rydr-safe-logo.png', width: 1701, height: 925 },
  dp: { src: '/branding/rydr-safe-dp.jpg', width: 474, height: 265, rounded: true },
}

// rydr-safe-logo.png has ~57% transparent padding baked into the canvas
// (measured content bbox 1362×396 inside the 1701×925 file) — sizing the raw
// image left most of the box empty no matter how tall it was set. Cropping to
// the mark's own aspect ratio via object-cover removes that dead space at
// render time, without touching the source file.
const FULL_LOGO_CROP = 'aspect-[1362/396] object-cover object-center'

export function Logo({ size = 'md', variant = 'full', className }: { size?: LogoSize; variant?: LogoVariant; className?: string }) {
  const asset = VARIANT_ASSET[variant]
  return (
    <img
      src={asset.src}
      alt="RydrSafe"
      width={asset.width}
      height={asset.height}
      className={cn(SIZE_CLASSES[size], 'w-auto shrink-0', variant === 'full' && FULL_LOGO_CROP, asset.rounded && 'rounded-xl', className)}
    />
  )
}
