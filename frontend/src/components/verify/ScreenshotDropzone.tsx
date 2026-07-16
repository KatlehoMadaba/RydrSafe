import { useEffect, useMemo, useRef, useState } from 'react'
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

/** Confirm-step previews. Object URLs are created per file list and revoked on cleanup — never leaked across re-renders. */
export function ScreenshotThumbnails({ files }: { files: File[] }) {
  const urls = useMemo(() => files.map((f) => URL.createObjectURL(f)), [files])

  useEffect(() => {
    return () => urls.forEach((u) => URL.revokeObjectURL(u))
  }, [urls])

  return (
    <div className="grid grid-cols-3 gap-2">
      {urls.map((url, i) => (
        <img key={url} src={url} alt={`Screenshot ${i + 1}`} className="aspect-square w-full rounded-lg border border-border object-cover" />
      ))}
    </div>
  )
}
