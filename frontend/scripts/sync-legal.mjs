// Copies the canonical user agreement into the static assets the SPA serves.
//
// There were two copies of this document and they had drifted: the one under docs/legal was
// corrected, while the one the app actually served still carried the wrong POPIA citations and
// a claim about OCR that was untrue. A legal notice that disagrees with itself depending on
// where you read it is worse than having only one.
//
// Running this on every build means the served copy cannot fall behind the source of truth.

import { copyFile, mkdir, readFile } from 'node:fs/promises'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'

const here = dirname(fileURLToPath(import.meta.url))
const source = resolve(here, '../../docs/legal/user-agreement.md')
const destination = resolve(here, '../public/legal/user-agreement.md')

let text
try {
  text = await readFile(source, 'utf8')
} catch (error) {
  // Failing loudly is the point: silently leaving the previously-synced copy in place is how
  // the two versions drifted apart in the first place.
  console.error(
    `\nCannot read the canonical user agreement at:\n  ${source}\n\n`
    + 'The build runs from frontend/ but this file lives at the repository root, so a build\n'
    + 'that only checks out frontend/ will not find it.\n',
  )
  throw error
}

const tokens = text.match(/TBD:/g) ?? []

if (tokens.length > 0) {
  // Who is allowed to ship a draft, and who is not.
  //
  // The document is a binding agreement with placeholders still in it. Publishing that to
  // production would put a draft in front of users as though it were final. Publishing it to a
  // preview would not — a preview is how you review the thing before it is final.
  //
  // Failing every build, as this used to, turned every pull request red for a state everyone
  // already knew about. A check that is always red is a check nobody reads, which costs more
  // safety than it buys. So enforcement keys off the deploy context each platform reports.
  const isProduction =
    process.env.CONTEXT === 'production' // Netlify: production | deploy-preview | branch-deploy | dev
    || process.env.VERCEL_ENV === 'production' // Vercel: production | preview | development
    || process.env.ENFORCE_FINAL_LEGAL === 'true' // force the strict path anywhere

  // Last-resort override, for the case where production must ship with the draft knowingly.
  const overridden = process.env.ALLOW_DRAFT_LEGAL === 'true'

  if (isProduction && !overridden) {
    console.error(
      `\nRefusing to publish the user agreement to production: ${tokens.length} unresolved `
      + 'TBD: token(s) remain.\n\n'
      + 'The agreement still contains placeholders (company registration, operator regions,\n'
      + 'the section 57 application status). Resolve them — see\n'
      + 'docs/legal/COMPLIANCE-NOTES.md section 5 — before this goes live.\n\n'
      + 'To ship anyway, knowingly, set ALLOW_DRAFT_LEGAL=true.\n',
    )
    process.exit(1)
  }

  console.warn(
    `\n⚠  User agreement is still a DRAFT: ${tokens.length} unresolved TBD: token(s).\n`
    + '   Fine for a preview or a local build. A production deploy will refuse it until they\n'
    + '   are resolved (docs/legal/COMPLIANCE-NOTES.md section 5).\n',
  )
}

await mkdir(dirname(destination), { recursive: true })
await copyFile(source, destination)

console.log('Synced user-agreement.md into public/legal/')
