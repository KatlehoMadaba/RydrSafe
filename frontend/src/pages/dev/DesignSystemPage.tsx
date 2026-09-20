import { useState } from 'react'
import {
  ShieldCheck,
  Clock,
  ShieldAlert,
  OctagonAlert,
  SearchX,
  Bell,
  Flag,
  Info,
  type LucideIcon,
} from 'lucide-react'

import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Progress } from '@/components/ui/progress'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Textarea } from '@/components/ui/textarea'
import { VerificationRing, type RingTone } from '@/components/ui/verification-ring'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { RiskScore } from '@/components/RiskScore'
import { cn } from '@/lib/utils'

/**
 * The design system review surface. DEV-only — App.tsx gates the route on
 * import.meta.env.DEV, so this never ships.
 *
 * This is the gate described in the redesign plan: the primitive library must be
 * consistent here before any page-level redesign begins.
 */

// Class names are spelled out in full: Tailwind scans source as plain text, so
// anything built by interpolation (`bg-${tone}`) is never generated.
const STATUS: {
  tone: RingTone
  label: string
  icon: LucideIcon
  score: number
  base: string
  soft: string
  muted: string
  strong: string
  iconColor: string
}[] = [
  {
    tone: 'safe',
    label: 'Safe',
    icon: ShieldCheck,
    score: 12,
    base: 'bg-safe',
    soft: 'bg-safe-soft',
    muted: 'border-safe-muted',
    strong: 'text-safe-strong',
    iconColor: 'text-safe',
  },
  {
    tone: 'review',
    label: 'Under review',
    icon: Clock,
    score: 44,
    base: 'bg-review',
    soft: 'bg-review-soft',
    muted: 'border-review-muted',
    strong: 'text-review-strong',
    iconColor: 'text-review',
  },
  {
    tone: 'flagged',
    label: 'Flagged',
    icon: ShieldAlert,
    score: 68,
    base: 'bg-flagged',
    soft: 'bg-flagged-soft',
    muted: 'border-flagged-muted',
    strong: 'text-flagged-strong',
    iconColor: 'text-flagged',
  },
  {
    tone: 'highrisk',
    label: 'High risk',
    icon: OctagonAlert,
    score: 91,
    base: 'bg-highrisk',
    soft: 'bg-highrisk-soft',
    muted: 'border-highrisk-muted',
    strong: 'text-highrisk-strong',
    iconColor: 'text-highrisk',
  },
  {
    tone: 'norecord',
    label: 'No community record',
    icon: SearchX,
    score: 0,
    base: 'bg-norecord',
    soft: 'bg-norecord-soft',
    muted: 'border-norecord-muted',
    strong: 'text-norecord-strong',
    iconColor: 'text-norecord',
  },
]

const NAVY_SWATCHES = [
  'bg-navy-50',
  'bg-navy-100',
  'bg-navy-300',
  'bg-navy-500',
  'bg-navy-700',
  'bg-navy-900',
]

const TEAL_SWATCHES = [
  'bg-teal-50',
  'bg-teal-100',
  'bg-teal-300',
  'bg-teal-500',
  'bg-teal-700',
  'bg-teal-900',
]

function Section({ title, hint, children }: { title: string; hint?: string; children: React.ReactNode }) {
  return (
    <section className="space-y-4">
      <div className="space-y-1">
        <h2 className="font-display text-2xl font-bold tracking-tight">{title}</h2>
        {hint ? <p className="text-sm text-muted-foreground">{hint}</p> : null}
      </div>
      <Separator />
      {children}
    </section>
  )
}

function Swatch({ name, className, note }: { name: string; className: string; note?: string }) {
  return (
    <div className="space-y-1.5">
      <div className={cn('h-14 rounded-lg border border-border', className)} />
      <div>
        <p className="font-mono text-xs text-foreground">{name}</p>
        {note ? <p className="text-xs text-muted-foreground">{note}</p> : null}
      </div>
    </div>
  )
}

export function DesignSystemPage() {
  const [density, setDensity] = useState<'comfortable' | 'compact'>('comfortable')

  return (
    <div className="min-h-screen bg-background">
      <div className="sticky top-0 z-10 border-b border-border bg-card/90 backdrop-blur">
        <div className="mx-auto flex max-w-5xl items-center justify-between gap-4 px-6 py-4">
          <div>
            <h1 className="font-display text-xl font-extrabold tracking-tight">RydrSafe design system</h1>
            <p className="text-xs text-muted-foreground">Calm Guardian — dev only</p>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Density</span>
            <div className="flex gap-1 rounded-lg bg-muted p-1">
              {(['comfortable', 'compact'] as const).map((d) => (
                <button
                  key={d}
                  onClick={() => setDensity(d)}
                  className={cn(
                    'rounded-md px-3 py-1 text-xs font-medium capitalize transition-colors',
                    density === d ? 'bg-card text-foreground shadow-xs' : 'text-muted-foreground hover:text-foreground'
                  )}
                >
                  {d}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Density is applied exactly as AppShell will apply it, so what you review
          here is what pages will render. */}
      <main data-density={density} className="mx-auto max-w-5xl space-y-14 px-6 py-10">
        <Section title="Typography" hint="Manrope for display and headings, Inter for body.">
          <div className="space-y-3">
            <p className="font-display text-3xl font-extrabold tracking-tight md:text-4xl">
              Display — Verify your driver
            </p>
            <p className="font-display text-2xl font-bold tracking-tight">Heading — Flagged by the community</p>
            <p className="font-display text-lg font-semibold">Subheading — Recent verifications</p>
            <p className="text-sm md:text-base">
              Body — Check a driver's community safety record before you get in the car.
            </p>
            <p className="text-xs text-muted-foreground">Caption — Checked just now</p>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Label — Registration</p>
          </div>
        </Section>

        <Section title="Brand" hint="Navy carries identity. Teal carries interaction and focus.">
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-6">
            {NAVY_SWATCHES.map((c) => (
              <Swatch key={c} name={c.replace('bg-', '')} className={c} />
            ))}
            {TEAL_SWATCHES.map((c) => (
              <Swatch key={c} name={c.replace('bg-', '')} className={c} />
            ))}
          </div>
        </Section>

        <Section title="Surfaces" hint="Near-white app background, white cards, soft gray secondary surfaces.">
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <Swatch name="background" className="bg-background" note="#F7F9FB" />
            <Swatch name="card" className="bg-card" note="#FFFFFF" />
            <Swatch name="muted / secondary" className="bg-muted" note="#F1F4F7" />
            <Swatch name="accent" className="bg-accent" note="hover surface" />
          </div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <Swatch name="foreground" className="bg-foreground" note="15:1 on white" />
            <Swatch name="muted-foreground" className="bg-muted-foreground" note="4.9:1 — AA floor" />
            <Swatch name="subtle" className="bg-subtle" note="2.8:1 — decorative only" />
            <Swatch name="ring" className="bg-ring" note="teal focus" />
          </div>
        </Section>

        <Section
          title="Status ramps"
          hint="Five states, five colours, five icons. Colour never carries meaning alone."
        >
          <div className="space-y-3">
            {STATUS.map(({ tone, label, icon: Icon, base, soft, muted, strong, iconColor }) => (
              <div key={tone} className="grid grid-cols-2 items-center gap-3 sm:grid-cols-6">
                <div className="flex items-center gap-2">
                  <Icon className={cn('h-4 w-4', iconColor)} />
                  <span className="font-mono text-xs">{tone}</span>
                </div>
                <div className={cn('h-10 rounded-md', base)} title="base" />
                <div className={cn('h-10 rounded-md', soft)} title="soft" />
                <div className={cn('h-10 rounded-md border-2 bg-card', muted)} title="muted (border)" />
                <div className={cn('flex h-10 items-center justify-center rounded-md', soft)}>
                  <span className={cn('text-xs font-semibold', strong)}>strong on soft</span>
                </div>
                <Badge variant={tone} icon={Icon}>
                  {label}
                </Badge>
              </div>
            ))}
          </div>
        </Section>

        <Section
          title="Verification ring"
          hint="The brand motif. Decorative and aria-hidden — the score always appears as text alongside."
        >
          <div className="flex flex-wrap items-end gap-8">
            {STATUS.map(({ tone, icon, score }) => (
              <div key={tone} className="space-y-2 text-center">
                <VerificationRing
                  tone={tone}
                  icon={icon}
                  score={score}
                  size="lg"
                  variant={tone === 'norecord' ? 'pending' : 'score'}
                />
                <p className="font-mono text-xs text-muted-foreground">
                  {tone === 'norecord' ? 'pending' : `${score}/100`}
                </p>
              </div>
            ))}
          </div>
          <div className="flex flex-wrap items-end gap-8 pt-4">
            {(['xs', 'sm', 'md', 'lg'] as const).map((size) => (
              <div key={size} className="space-y-2 text-center">
                <VerificationRing tone="flagged" icon={ShieldAlert} score={68} size={size} />
                <p className="font-mono text-xs text-muted-foreground">{size}</p>
              </div>
            ))}
            <div className="space-y-2 text-center">
              <VerificationRing tone="norecord" score={0} size="lg" variant="scanning" />
              <p className="font-mono text-xs text-muted-foreground">scanning</p>
            </div>
          </div>
        </Section>

        <Section title="Buttons" hint="Default targets clear 44px. Compact trades that for density.">
          <div className="flex flex-wrap items-center gap-3">
            {(['default', 'destructive', 'outline', 'secondary', 'soft', 'ghost', 'link'] as const).map((v) => (
              <Button key={v} variant={v}>
                {v}
              </Button>
            ))}
          </div>
          <div className="flex flex-wrap items-center gap-3">
            {(['sm', 'default', 'lg'] as const).map((s) => (
              <Button key={s} size={s}>
                size {s}
              </Button>
            ))}
            <Button size="icon" aria-label="Notifications">
              <Bell className="h-4 w-4" />
            </Button>
            <Button disabled>disabled</Button>
            <Button isLoading>loading</Button>
            <Button variant="outline" isLoading>
              loading
            </Button>
          </div>
        </Section>

        <Section title="Badges" hint="Legacy variants remain until callsites migrate to the status variants.">
          <div className="flex flex-wrap items-center gap-3">
            {(['default', 'secondary', 'outline', 'success', 'warning', 'destructive'] as const).map((v) => (
              <Badge key={v} variant={v}>
                {v}
              </Badge>
            ))}
          </div>
          <div className="flex flex-wrap items-center gap-3">
            {STATUS.map(({ tone, label, icon }) => (
              <Badge key={tone} variant={tone} icon={icon}>
                {label}
              </Badge>
            ))}
          </div>
        </Section>

        <Section title="Alerts" hint="Emphasis comes from the weighted left border, not from shouting.">
          <div className="space-y-3">
            <Alert>
              <Info />
              <AlertTitle>Default</AlertTitle>
              <AlertDescription>A neutral, informational callout.</AlertDescription>
            </Alert>
            <Alert variant="safe">
              <ShieldCheck />
              <AlertTitle>No reports on record</AlertTitle>
              <AlertDescription>Ordinary care still applies.</AlertDescription>
            </Alert>
            <Alert variant="review">
              <Clock />
              <AlertTitle>Some concerns have been raised</AlertTitle>
              <AlertDescription>Check the details before you ride.</AlertDescription>
            </Alert>
            <Alert variant="flagged">
              <ShieldAlert />
              <AlertTitle>Several passengers have reported this driver</AlertTitle>
              <AlertDescription>Consider requesting a different driver.</AlertDescription>
            </Alert>
            <Alert variant="highrisk">
              <OctagonAlert />
              <AlertTitle>We recommend you do not continue this trip</AlertTitle>
              <AlertDescription>
                Cancel and request a different driver. If you feel unsafe, contact local emergency services.
              </AlertDescription>
            </Alert>
            <Alert variant="norecord">
              <SearchX />
              <AlertTitle>No community record yet</AlertTitle>
              <AlertDescription>
                Nobody has verified or reported this driver yet. That isn't the same as a clean record.
              </AlertDescription>
            </Alert>
          </div>
        </Section>

        <Section title="Cards & elevation" hint="Navy-tinted shadows — never pure black.">
          <div className="grid gap-4 sm:grid-cols-3">
            <Card>
              <CardHeader>
                <CardTitle>Card title</CardTitle>
                <CardDescription>Supporting description text.</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">Padding responds to density.</p>
              </CardContent>
            </Card>
            <div className="space-y-3">
              {(['shadow-xs', 'shadow-sm', 'shadow-md', 'shadow-lg', 'shadow-xl'] as const).map((s) => (
                <div key={s} className={cn('rounded-lg border border-border bg-card p-3 text-xs font-mono', s)}>
                  {s}
                </div>
              ))}
            </div>
            <div className="space-y-3">
              {(['rounded-sm', 'rounded-md', 'rounded-lg', 'rounded-xl', 'rounded-2xl'] as const).map((r) => (
                <div key={r} className={cn('border border-border bg-muted p-3 text-xs font-mono', r)}>
                  {r}
                </div>
              ))}
            </div>
          </div>
        </Section>

        <Section title="Forms" hint="Inputs are 44px, compacting to 36px. Focus is a teal ring.">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="ds-reg">Registration number</Label>
              <Input id="ds-reg" placeholder="e.g. GP12ABGP" />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="ds-sev">Severity</Label>
              <Select>
                <SelectTrigger id="ds-sev">
                  <SelectValue placeholder="Select severity" />
                </SelectTrigger>
                <SelectContent>
                  {['Low', 'Medium', 'High', 'Critical'].map((s) => (
                    <SelectItem key={s} value={s}>
                      {s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5 sm:col-span-2">
              <Label htmlFor="ds-desc">Description</Label>
              <Textarea id="ds-desc" placeholder="What happened?" />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="ds-dis">Disabled</Label>
              <Input id="ds-dis" placeholder="Disabled" disabled />
            </div>
          </div>
        </Section>

        <Section title="Tabs, dialog, progress, skeleton">
          <Tabs defaultValue="screenshot">
            <TabsList>
              <TabsTrigger value="screenshot">Screenshot</TabsTrigger>
              <TabsTrigger value="manual">Manual entry</TabsTrigger>
            </TabsList>
            <TabsContent value="screenshot">
              <p className="pt-2 text-sm text-muted-foreground">Arrow keys move between tabs.</p>
            </TabsContent>
            <TabsContent value="manual">
              <p className="pt-2 text-sm text-muted-foreground">Manual entry panel.</p>
            </TabsContent>
          </Tabs>

          <div className="flex flex-wrap items-center gap-6 pt-2">
            <Dialog>
              <DialogTrigger asChild>
                <Button variant="outline">Open dialog</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Dialog title</DialogTitle>
                  <DialogDescription>Fades and scales in gently. Esc closes.</DialogDescription>
                </DialogHeader>
              </DialogContent>
            </Dialog>
            <LoadingSpinner />
            <Button variant="ghost">
              <Flag className="h-4 w-4" /> With icon
            </Button>
          </div>

          <div className="grid gap-4 pt-2 sm:grid-cols-2">
            <div className="space-y-3">
              <Progress value={30} />
              <Progress value={68} indicatorClassName="bg-flagged" />
              <Progress value={91} indicatorClassName="bg-highrisk" />
              {STATUS.filter((s) => s.tone !== 'norecord').map((s) => (
                <RiskScore key={s.tone} score={s.score} />
              ))}
            </div>
            <div className="space-y-2">
              <Skeleton className="h-8 w-2/3" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-5/6" />
              <Skeleton className="h-24 w-full" />
            </div>
          </div>
        </Section>
      </main>
    </div>
  )
}
