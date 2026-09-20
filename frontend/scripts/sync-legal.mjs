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

const text = await readFile(source, 'utf8')

// The agreement carries TBD: tokens for every value still to be decided. Shipping it in that
// state would publish a draft as though it were binding, so the build refuses unless the
// release explicitly opts in.
if (text.includes('TBD:') && process.env.ALLOW_DRAFT_LEGAL !== 'true') {
  const count = text.match(/TBD:/g).length

  console.error(
    `\nRefusing to publish the user agreement: ${count} unresolved TBD: token(s) remain.\n`
    + 'Resolve them (see docs/legal/COMPLIANCE-NOTES.md §5), or set ALLOW_DRAFT_LEGAL=true\n'
    + 'to build anyway for a preview deploy.\n',
  )
  process.exit(1)
}

await mkdir(dirname(destination), { recursive: true })
await copyFile(source, destination)

console.log('Synced user-agreement.md into public/legal/')
