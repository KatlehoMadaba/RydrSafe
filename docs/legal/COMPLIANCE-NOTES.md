# RydrSafe — Compliance Implementation Notes

**Internal. Not user-facing. Do not publish.**

The user-facing document is [user-agreement.md](user-agreement.md). It makes concrete promises about how the product behaves. This file tracks whether each promise is true of the codebase, and what still has to be built, decided, or signed before the agreement can go in front of a user or a lawyer.

Nothing here is legal advice. This is a checklist to hand to an attorney, not a substitute for one.

---

## 1. Implementation status

### 1.1 Built

These were the original B1–B12 blockers. They are implemented and the agreement now describes them accurately.

| # | Item | Where |
|---|---|---|
| B1 | **Publication tiers.** `ReportStatus` is `Pending → Approved → Corroborated`, plus terminal `Rejected` and `Withdrawn`. Only `Corroborated` is publicly visible or counted. | `ReportStatus.cs`, `ReportRepository.cs` |
| B2 | **Corroboration threshold.** All three limbs of clause 6.3, with independence testing, and revocation when a supporting path is withdrawn. | `CorroborationPolicy.cs`, `CorroborationService.cs` |
| B3 | **Category A/B classification.** Derived from the category at submission; `Other` defaults to Category A and only `Other` may be reclassified. | `ReportClassificationPolicy.cs` |
| B4 | **Free text never published.** The full `ReportDto` is reachable only by the reporter or a moderator; everyone else gets `PublicReportSummaryDto`. | `GetReportByIdQuery.cs`, `GetPublicReportSummariesQuery.cs` |
| B5 | **Category A gate.** Config-level, defaults closed, and gates submission — not just publication. | `CategoryAGate.cs` |
| B6 | **Human-in-the-loop.** No code path changes a report or driver status without an actor, a reason, and review confirmations. Append-only audit tables. | `ModerateReportCommand.cs`, `SetDriverStatusCommand.cs` |
| B7 | **Driver self-check + appeals.** Unauthenticated, rate limited, uniform responses, every lookup logged. | `DriverSelfCheckQuery.cs`, `CreateAppealCommand.cs` |
| B8 | **Right of reply.** An adverse status change is refused unless the notice was sent first. | `DriverStatusCommands.cs` |
| B9 | **Consent capture.** One stored row per checkbox, with version, locale, surface, timestamp and IP. | `UserConsent.cs`, `RegisterCommand.cs` |
| B10 | **Age gate.** Date of birth captured and validated server-side. | `RegisterCommandValidator.cs` |
| B11 | **Image handling.** Images never reach disk or database; SHA-256 retained for duplicate detection; the discard is timestamped. | `OcrService.cs` |
| B12 | **Retention jobs.** A scheduled worker enforces the periods it can. | `RetentionWorker.cs` |

Two things changed behaviour for existing data, and the migration handles both: reports previously marked `Escalated` return to `Pending`, and every driver's risk score and status reset to zero/`Safe`, because the old scores were computed from uncorroborated reports.

### 1.2 Still open

| # | Item | Why it matters |
|---|---|---|
| C1 | **Account-data retention is not automated.** Clause 29 marks these rows ⚠️. Closure, report expiry, audit-log expiry and consent expiry are honoured on request but not by a job. | A stated period with no job behind it is a misrepresentation. |
| C2 | **No legal-hold flag.** Clause 29 promises records under legal hold are exempt from purging. Nothing marks a record as held. | The retention worker would purge evidence in a live dispute. |
| C3 | **Identity verification on appeals is manual.** `DriverAppeal.IdentityVerified` exists but nothing enforces it before an appeal suspends a status. | Anyone who knows a registration number can suspend that driver's status. Currently mitigated only by rate limiting. Decide whether that trade-off is acceptable — it favours the driver, which is the safer direction, but it is abusable. |
| C4 | **Moderator confidentiality undertaking** is referenced in clauses 1.3 and 27.2 but does not exist as a document. |
| C5 | **Consent re-acceptance on version change.** Clause 17.1 promises users are asked to accept a materially changed version. Nothing compares the stored version against the current one at sign-in. | Existing accounts predate Part D entirely and hold no consent rows. |
| C6 | **Notifications are in-process only.** `NotifyModeratorsAsync` writes rows and pushes over SignalR, with no email fallback. Clause 6.5 notice to a driver has no delivery channel — we hold a phone number at best, and nothing sends to it. | The right of reply is gated in code but cannot actually reach a driver yet. |

---

## 2. The special-personal-information question (the real one)

This is the issue that no drafting fixes, and it is the one to take to an attorney first.

- **POPIA s26(b)** prohibits processing personal information about a person's alleged commission of an offence, unless authorised under Chapter 3 Part B.
- **s27(1)** lists the general authorisations. The one the agreement relies on is **s27(1)(b)** — necessary for the establishment, exercise or defence of a right or obligation in law. This is arguable, not settled. It is strongest when the processing is narrow and proportionate, which is exactly why clauses 6.3 and 6.4 matter.
- **s33** authorises processing of criminal-behaviour information by bodies charged by law with applying criminal law, or by responsible parties who obtained it in accordance with the law. RydrSafe is not the former. Whether it is the latter is the question.
- **s57(1)(b)** — this is the sharp one. Prior authorisation from the Information Regulator is required before processing information on criminal or unlawful conduct **on behalf of third parties**. RydrSafe processes allegations submitted by users, about drivers, for the benefit of other users. That reads squarely onto s57(1)(b).
  *(An earlier draft cited s57(1)(d). That subsection concerns transferring special personal information to a third country — a real obligation for us given clause 27.4, but a different one.)*
- **s58(2)** means the processing may not begin until the Regulator has completed its assessment, or has notified us that a detailed assessment will not be conducted.
- **s58(7)** caps the delay: if the Regulator does not decide within the prescribed period, processing may proceed. Do not rely on this without counsel confirming the period has run — the statutory fallback and our own product rule are separate things, and we have chosen the stricter one.

**The standstill covers processing, not publication.** Submitting, storing, moderating, matching and scoring are all processing. `CategoryAGate.IsProcessingEnabled` therefore gates submission: a Category A report is refused at the API with a 503 and an explanation, and nothing is written. An earlier version of this file said Category A reports could be "retained but not published" during the standstill; that was wrong, and the code no longer works that way.

**Recommended position:** apply for prior authorisation, ship with Category A processing switched off, and run on Category B only in the meantime. That is a shippable product and a defensible one.

## 3. Documents that must exist before launch

| Document | Basis | Status |
|---|---|---|
| **Legitimate Interests Assessment (LIA)** | s11(1)(f). Clause 23.2 now says explicitly that this is *not* done, rather than asserting it. Write it before that clause is softened. Must weigh passenger safety against the driver's rights and record the safeguards relied on. | ☐ |
| **Operator agreements (s21)** | s21(1) requires a written contract obliging s19 security measures; s21(2) requires the operator to notify us immediately of suspected unauthorised access. Needed for **Google Cloud (Vision API), Render, Supabase, Netlify**, plus email and error-monitoring providers once chosen. | ☐ |
| **Google Vision data handling** | Specifically: retention period for submitted images, whether they are used for anything beyond returning a result, and Google's own subprocessors. Clause 27.3 cannot be finalised without it. | ☐ |
| **Cross-border transfer record (s72)** | Per recipient: region, subprocessors, categories transferred, onward transfer, deletion on termination, and the s72 ground relied on. The clause 27.4 table is the template; every cell is still `TBD:`. | ☐ |
| **Information Officer registration** | s55/s56. Registration with the Regulator is mandatory, and free. | ☐ |
| **PAIA manual** | s51 **PAIA** (not POPIA — POPIA s51 concerns Regulator meetings). The blanket exemption for private bodies lapsed on 31 December 2021. | ☐ |
| **s57 prior authorisation application** | See §2. | ☐ |
| **Data breach response plan** | s22 requires notification "as soon as reasonably possible". Have the runbook before you need it. | ☐ |
| **Moderator confidentiality undertaking** | Clauses 1.3 and 27.2 both reference it (C4). | ☐ |
| **CPA applicability opinion** | Whether RydrSafe is supplied "in the ordinary course of business for consideration" determines whether the s49 notice and s51 carve-outs in clause 15 are the right framing. | ☐ |
| **Records of processing** | Good practice; the Regulator asks for it. | ☐ |

## 4. Positions taken in the drafting, and why

**ECTA safe harbour.** Chapter XI (ss 70–79) limits liability for service providers hosting third-party content. Two things are often conflated and clause 12.3 now separates them. First, the protections are only available to members of a Ministerially-recognised industry representative body that has adopted a code of conduct — in practice ISPA — and we are not a member. Second, s75 makes the limitation conditional on knowledge, on expeditious removal after notification, and on having a designated agent; s78 confirms there is no general duty to monitor. Moderating does not automatically defeat s75. What it does is change the facts the s75 conditions are assessed against, because the more closely we review content the harder it is to say we lacked knowledge of it. The agreement resolves this by **not claiming the safe harbour as the primary defence** and reserving it without prejudice. The real protection is the corroboration threshold. If you join ISPA, revisit clause 12.3 — it would then be worth claiming properly, and the designated-agent address in clause 12.1 becomes load-bearing.

**Liability.** The original draft excluded personal injury outright. **CPA s51(1)(c)** prohibits a term excluding liability for loss attributable to gross negligence, and s49 requires that risk-limiting terms be conspicuous and separately drawn to the consumer's attention. Clause 15 therefore carves out death, personal injury, gross negligence, intent and fraud; limits what remains; and carries a boxed s49 notice with its own checkbox in Part D. A blanket exclusion would likely have been struck down and would have made the whole clause look bad-faith.

**Whether the CPA applies at all** depends on whether RydrSafe is supplied "in the ordinary course of business for consideration". A free app may fall outside the definition of a transaction. The agreement is drafted as though the CPA applies, which is the safe direction, and clause 15 now says so in a drafting note that must be removed before publication.

**Defamation.** SA common law presumes publication of defamatory matter to be wrongful. The relevant defence here is reasonable publication (*National Media Ltd v Bogoshi* 1998 (4) SA 1196 (SCA)). Every element of clause 6 is aimed at it: corroboration before publication, no free-text narrative, right of reply, prompt take-down, human review. That is what "reasonable publication" looks like in practice.

**POPIA s18.** Driver information is collected from users, not from drivers. s18 requires reasonably practicable steps to notify. Clause 21 relies on a public notice, a driver self-check page, and direct notice before a status change. The self-check page now exists. Direct notice does not have a delivery channel yet (C6), so the s18 position currently rests on the public notice and the self-check.

**POPIA s71.** A risk score that changes a driver's public standing is automated processing affecting a person to a substantial degree. Recording a moderator's click is not by itself meaningful human involvement — it evidences that someone acted, not that anyone thought. The commands therefore require the moderator to affirm what they reviewed (the report content, the driver's response where one exists, the score and how it was derived), refuse an adverse change where the driver has responded and the moderator has not confirmed reading it, and store those affirmations on the audit row. That is the most we can actually evidence in software; whether it satisfies s71(2)(b) is a judgement for counsel.

**Independence signals.** Clause 6.3(a) originally spoke of linkage by "device, network, payment identity or referral". We have no payment system and no referral system, so two of those four were undisclosed capabilities we did not possess. The clause now names the two signals we do collect — salted hashes of IP address and device identifier — and Part B, clause 20.1 discloses them with purpose, retention and access. If a matching input is ever sourced from another responsible party, s57(1)(a) prior authorisation becomes a separate question.

**Anti-enumeration.** An unauthenticated lookup keyed on registration number is an oracle for "which drivers does RydrSafe hold". The controls are: identical response text on hit and miss, a required contact address, a per-IP rate limit, and an access-log row for every attempt including misses. The appeal form fails the same way as a bad registration number for the same reason. What is *not* implemented is proof of identity (C3).

**Copyright / trade marks.** Real but lower priority. The mitigation is minimisation — images discarded after OCR, no operator branding in the RydrSafe UI, nominative reference only. The realistic risk is a cease-and-desist from an operator, not a regulator complaint. Clause 10 covers it in a paragraph, which is proportionate.

**Xenophobia.** Clause 8.6 is deliberate. A community reporting platform aimed at e-hailing drivers in South Africa, with no such clause, will be used to target foreign-national drivers. This needs to be a moderation rule with teeth, not just a line in the terms.

## 5. Pre-publication check

Every unresolved value in the agreement is written as `TBD: SOME_NAME`. That token appears nowhere else in the document, so the release check is:

```sh
! grep -q 'TBD:' docs/legal/user-agreement.md
```

Searching for a bare `[` does not work — it matches every Markdown link.

**This check runs on every frontend build.** `frontend/scripts/sync-legal.mjs` copies this
directory's agreement into `frontend/public/legal/` — there were two copies and they had
drifted, with the served one still carrying the wrong POPIA citations — and it **fails the
build** while any `TBD:` token remains, rather than publishing a draft as though it were
binding.

⚠️ **Deployment consequence.** `ALLOW_DRAFT_LEGAL=true` is set in `frontend/vercel.json` so
preview deploys of this branch keep working. **`main` carries a `netlify.toml` that does not
set it**, so merging this branch without adding that variable there will fail the Netlify
build. Either add it to `netlify.toml` under `[build.environment]` with a note to remove it
before launch, or resolve the tokens first. The variable must come out before the real
launch — that is the point of the check.

The ones that need a decision rather than a lookup:

- `TBD: S57_STATUS` (clause 24.3) — pick one of the three options, and do not publish until it is settled.
- `TBD: LEGAL_FORM` — determines whether there is a company to be the responsible party at all.
- Regions and DPA status in clauses 27.3 and 27.4 — determine whether s72 applies and on what ground.
- `TBD: BACKUP_ROTATION_DAYS` (clause 29.3) — cannot be stated until the backup configuration is known.
- The Information Regulator's address in clause 30 was verified against inforegulator.org.za on 2026-09-20. Re-check at each annual review.

## 6. Suggested order of work

1. Decide the s57 prior-authorisation question with an attorney. Everything else is downstream of it.
2. Write the LIA, then soften clause 23.2 to match.
3. Pin down the operator and cross-border rows (§3) — these are lookups and signatures, not engineering.
4. Give the right of reply a delivery channel (C6). It is gated in code but cannot reach anyone.
5. Automate the remaining retention periods and add a legal-hold flag (C1, C2).
6. Decide the appeal identity-verification trade-off (C3).
7. Add consent re-acceptance on version change (C5).
8. Have an attorney review the agreement *after* the above, so they are reviewing something true.
