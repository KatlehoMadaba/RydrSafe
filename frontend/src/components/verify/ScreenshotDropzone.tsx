import { useEffect, useRef, useState } from 'react'
import { Upload, X } from 'lucide-react'
import { cn } from '@/lib/utils'

interface ScreenshotDropzoneProps {
  files: File[]
  onAddFiles: (files: FileList | null) => void
  onRemoveFile: (index: number) => void
}

export function ScreenshotDropzone({ files, onAddFiles, onRemoveFile }: ScreenshotDropzoneProps) {
  const [isDragging, setIsDragging] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const openPicker = () => inputRef.current?.click()

  return (
    <div className="space-y-3">
      {/* Keyboard-reachable: the primary action of the app's primary page must
          not depend on a mouse. */}
      <div
        role="button"
        tabIndex={0}
        aria-label="Upload screenshots"
        className={cn(
          'border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors',
          isDragging ? 'border-teal-500 bg-teal-50' : 'border-border hover:border-teal-400'
        )}
        onClick={openPicker}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault()
            openPicker()
          }
        }}
        onDragOver={(e) => {
          e.preventDefault()
          setIsDragging(true)
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={(e) => {
          e.preventDefault()
          setIsDragging(false)
          onAddFiles(e.dataTransfer.files)
        }}
      >
        <Upload className="h-10 w-10 text-subtle mx-auto mb-3" />
        <p className="font-medium text-foreground">Drop screenshots here or click to browse</p>
        <p className="text-sm text-muted-foreground mt-1">Up to 3 files, max 10MB each</p>
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/jpeg,image/png,image/webp"
          className="hidden"
          // Reset the value so re-selecting a just-removed file still fires
          // change (browsers suppress it when the selected value is identical).
          onChange={(e) => {
            onAddFiles(e.target.files)
            e.target.value = ''
          }}
        />
      </div>
      {files.length > 0 && (
        <div className="space-y-2">
          {files.map((f, i) => (
            <div key={i} className="flex items-center justify-between bg-muted rounded-md px-3 py-2">
              <span className="text-sm text-foreground truncate">{f.name}</span>
              <button
                type="button"
                onClick={() => onRemoveFile(i)}
                className="shrink-0 text-subtle hover:text-highrisk ml-2"
                aria-label={`Remove ${f.name}`}
              >
                <X className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

/**
 * Confirm-step previews.
 *
 * The object URLs are created inside the effect rather than in a `useMemo`, so the cleanup can
 * only ever revoke URLs that same effect created. Under StrictMode React mounts, runs cleanup,
 * then mounts again — but a memoised value survives that cycle untouched. The first cleanup
 * therefore revoked the very URLs still bound to the `<img>` tags, and nothing recreated them,
 * so every preview rendered as a broken image. Creating them in the effect means the remount
 * makes fresh URLs.
 */
export function ScreenshotThumbnails({
  files,
  onRemove,
}: {
  files: File[]
  /** Omit to render read-only previews. */
  onRemove?: (index: number) => void
}) {
  const [urls, setUrls] = useState<string[]>([])

  useEffect(() => {
    const created = files.map((f) => URL.createObjectURL(f))
    // The blob registry is an external system, and creating a URL has to be paired with the
    // cleanup that revokes it — which is the case this rule exempts. Deriving these during
    // render instead is precisely what produced the broken previews this replaces.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUrls(created)
    return () => created.forEach((u) => URL.revokeObjectURL(u))
  }, [files])

  return (
    <div className="grid grid-cols-3 gap-2">
      {urls.map((url, i) => (
        <div key={url} className="relative">
          <img
            src={url}
            alt={files[i]?.name ?? `Screenshot ${i + 1}`}
            className="aspect-square w-full rounded-lg border border-border object-cover"
          />
          {onRemove && (
            <button
              type="button"
              onClick={() => onRemove(i)}
              // Spotting the wrong screenshot is most likely here, at the last look before
              // upload, so the correction has to be available here rather than only a step back.
              aria-label={`Remove ${files[i]?.name ?? `screenshot ${i + 1}`}`}
              className="absolute right-1 top-1 rounded-full bg-navy-900/70 p-1 text-white transition-colors hover:bg-destructive focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}
          <p className="mt-1 truncate text-xs text-muted-foreground" title={files[i]?.name}>
            {files[i]?.name}
          </p>
        </div>
      ))}
    </div>
  )
}
