import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { Button } from '@/components/ui/button'
import { Logo } from '@/components/Logo'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ArrowLeft } from 'lucide-react'

/**
 * The user agreement, served as the same Markdown file the repository holds.
 *
 * It is fetched rather than bundled on purpose: `scripts/sync-legal.mjs` copies
 * `docs/legal/user-agreement.md` into `public/legal/` at build time, so the page and the
 * document in version control cannot drift. Inlining the text into a component would recreate
 * exactly the duplication that sync script exists to remove — there were two copies once, and
 * the served one carried the wrong POPIA citations.
 */
export function LegalPage() {
  const [markdown, setMarkdown] = useState<string | null>(null)
  const [failed, setFailed] = useState(false)

  // The agreement is published while placeholders remain, so the page has to say so. Detecting
  // it from the served text rather than a build flag means the notice cannot outlive the draft:
  // resolve the last TBD: and it disappears on its own.
  const isDraft = markdown?.includes('TBD:') ?? false

  useEffect(() => {
    let cancelled = false

    fetch('/legal/user-agreement.md')
      .then((r) => {
        if (!r.ok) throw new Error(String(r.status))
        return r.text()
      })
      .then((text) => {
        if (!cancelled) setMarkdown(text)
      })
      .catch(() => {
        if (!cancelled) setFailed(true)
      })

    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6 flex items-center justify-between gap-4">
        <Link to="/" aria-label="RydrSafe home">
          <Logo size="md" />
        </Link>
        <Button asChild variant="outline" size="sm">
          <Link to="/register">
            <ArrowLeft className="h-4 w-4" />
            Back to sign up
          </Link>
        </Button>
      </div>

      {isDraft && (
        <div className="mb-6 rounded-lg border border-review-muted bg-review-soft p-4">
          <p className="text-sm font-semibold text-review-strong">This agreement is a draft.</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Some details are still marked <code className="font-mono text-xs">TBD:</code> — including
            the operating entity, the Information Officer's contact details, and the data-processing
            arrangements with our service providers. Those sections are not yet settled and should
            not be relied on. Everything else describes how RydrSafe actually works today.
          </p>
        </div>
      )}

      {failed && (
        <div className="rounded-lg border border-highrisk-muted bg-highrisk-soft p-4">
          <p className="text-sm font-semibold text-highrisk-strong">
            The agreement could not be loaded.
          </p>
          <p className="mt-1 text-xs text-highrisk-strong/90">
            You can read it in the repository at <code>docs/legal/user-agreement.md</code>, or
            contact the Information Officer. Do not accept terms you have not been able to read.
          </p>
        </div>
      )}

      {!failed && markdown === null && (
        <div className="py-16">
          <LoadingSpinner />
        </div>
      )}

      {markdown && (
        // Tailwind's typography plugin is not installed, so the element styles are set here
        // rather than with `prose`. Tables need to scroll on a phone: this document has several
        // and they are wider than any handset.
        <article
          className="
            space-y-4 text-sm leading-relaxed text-foreground
            [&_a]:text-teal-600 [&_a]:underline-offset-4 hover:[&_a]:underline
            [&_blockquote]:border-l-4 [&_blockquote]:border-border [&_blockquote]:pl-4 [&_blockquote]:text-muted-foreground
            [&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_code]:py-0.5 [&_code]:text-xs
            [&_h1]:font-display [&_h1]:text-2xl [&_h1]:font-bold [&_h1]:mt-8 [&_h1]:mb-3
            [&_h2]:font-display [&_h2]:text-xl [&_h2]:font-bold [&_h2]:mt-8 [&_h2]:mb-2
            [&_h3]:text-base [&_h3]:font-semibold [&_h3]:mt-6 [&_h3]:mb-2
            [&_hr]:my-8 [&_hr]:border-border
            [&_li]:my-1
            [&_ol]:list-decimal [&_ol]:pl-6
            [&_strong]:font-semibold [&_strong]:text-foreground
            [&_table]:w-full [&_table]:text-xs
            [&_td]:border [&_td]:border-border [&_td]:p-2 [&_td]:align-top
            [&_th]:border [&_th]:border-border [&_th]:bg-muted [&_th]:p-2 [&_th]:text-left
            [&_ul]:list-disc [&_ul]:pl-6
          "
        >
          <ReactMarkdown
            remarkPlugins={[remarkGfm]}
            components={{
              table: ({ children }) => (
                <div className="my-4 overflow-x-auto">
                  <table>{children}</table>
                </div>
              ),
            }}
          >
            {markdown}
          </ReactMarkdown>
        </article>
      )}
    </div>
  )
}
