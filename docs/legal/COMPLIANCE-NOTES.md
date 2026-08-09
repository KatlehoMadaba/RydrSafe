# RydrSafe — Compliance Implementation Notes

**Internal. Not user-facing. Do not publish.**

The user-facing document is [user-agreement.md](user-agreement.md). It makes concrete promises about how the product behaves. **Several of those promises are not true of the current codebase yet.** This file lists what has to be built, decided, or signed before the agreement can go in front of a user or a lawyer.

Nothing here is legal advice. This is a checklist to hand to an attorney, not a substitute for one.

---

## 1. Blockers — the agreement is inaccurate until these are done

| # | Item | Why | Where |
|---|---|---|---|
| B1 | **Publication tiers** (Pending → Approved → Corroborated) | Clause 6.2 describes a three-stage lifecycle. The spec only has approve/reject. `Report.Status` needs the extra state, and public queries must filter on `Corroborated`, not `Approved`. | `Reports` table, `PUT /api/reports/{id}/approve` |
| B2 | **Corroboration threshold** | Clause 6.3. Category A cannot surface on one report. Needs: independence check across reports (device / IP / referral linkage), and an optional SAPS case-number field. | Matching + scoring service |
| B3 | **Category A/B classification** | Clause 6.1. Report categories need a criminal-allegation flag. `Other` defaults to Category A until reclassified. | `Report.Category` enum |
| B4 | **Free text never published** | Clause 6.4. The verification response must not carry `Description`. The spec's response shape is fine; make sure no DTO leaks it to a non-moderator role. | Verification DTOs |
| B5 | **Category A kill switch** | Clause 24.3. Until the s57 prior-authorisation question is settled, Category A publication must be *switchable off* at config level, not by code change. | Feature flag |
| B6 | **Human-in-the-loop on status change** | Clause 7.3 / POPIA s71. The score may be computed automatically; the public status band must require a recorded moderator action with a reason and an actor ID. | Scoring service, audit log |
| B7 | **Driver self-check + appeals** | Part C. There is currently no driver-facing surface at all. Needs an unauthenticated lookup with identity verification, and an appeals intake. | New public route |
| B8 | **Right of reply** | Clause 6.5. Notify the driver before a status change to Flagged/High Risk where a contact detail exists. | Notification service |
| B9 | **Consent capture** | Part D. `RegisterPage.tsx` and `RegisterCommand.cs` currently take name/email/password only. Need per-checkbox capture, agreement version, timestamp, IP — stored, not just validated client-side. | [RegisterPage.tsx](../../frontend/src/pages/public/RegisterPage.tsx), [RegisterCommand.cs](../../backend/RydrSafe.Application/Features/Auth/Commands/RegisterCommand.cs) |
| B10 | **Age gate** | Clause 3.1 / POPIA s34–35. No 18+ check exists. | Register flow |
| B11 | **Image hash + hard-delete guarantee** | Clause 25. Needs a perceptual or cryptographic hash retained for duplicate detection, and a verifiable delete path with a maximum age. | OCR service |
| B12 | **Retention jobs** | Clause 29 states specific periods. None are implemented. A stated retention period with no job behind it is worse than saying nothing. | Background worker |

## 2. The special-personal-information question (the real one)

This is the issue that no drafting fixes, and it is the one to take to an attorney first.

- **POPIA s26(b)** prohibits processing personal information about a person's alleged commission of an offence, unless authorised under Chapter 3 Part B.
- **s27(1)** lists the general authorisations. The one the agreement relies on is **s27(1)(b)** — necessary for the establishment, exercise or defence of a right or obligation in law. This is arguable, not settled. It is strongest when the processing is narrow and proportionate, which is exactly why clauses 6.3 and 6.4 matter.
- **s33** authorises processing of criminal-behaviour information by bodies charged by law with applying criminal law, or by responsible parties who obtained it in accordance with the law. RydrSafe is not the former. Whether it is the latter is the question.
- **s57(1)(d)** — this is the sharp one. Prior authorisation from the Information Regulator is required before processing information on criminal or unlawful conduct **on behalf of third parties**. RydrSafe processes allegations submitted by users, about drivers, for the benefit of other users. That reads squarely onto s57(1)(d).
- **s58(2)** means the processing may not begin until the Regulator has completed its assessment.

**Recommended position:** apply for prior authorisation, and keep Category A publication switched off (B5) until it resolves. Ship the platform with Category B publication and private Category A retention in the meantime. That is a shippable product and a defensible one.

**Do not** publish clause 24.3 with the placeholder text still in it.

## 3. Documents that must exist before launch

| Document | Basis | Status |
|---|---|---|
| **Legitimate Interests Assessment (LIA)** | s11(1)(f). Clause 23.2 says one exists. Write it *before* publishing that clause — asserting a lawful basis you have not assessed is evidence against you, not for you. Must weigh passenger safety interest against driver's rights, and record the safeguards relied on. | ☐ |
| **Operator agreements (s21(2))** | Written contract with every operator obliging s19 security measures. Azure: Microsoft's DPA / Data Protection Addendum — locate the executed version, do not assume. Plus email provider, error monitoring. | ☐ |
| **Information Officer registration** | s55/s56. Registration with the Regulator is mandatory, and free. | ☐ |
| **PAIA manual** | s51 PAIA. The blanket exemption for private bodies lapsed on 31 December 2021. A manual is required. | ☐ |
| **s57 prior authorisation application** | See section 2 above. | ☐ |
| **Data breach response plan** | s22 requires notification "as soon as reasonably possible". Have the runbook before you need it. | ☐ |
| **Moderator confidentiality undertaking** | Clause 1.3 and 27.2 both reference it. | ☐ |
| **Records of processing** | Good practice; the Regulator asks for it. | ☐ |

## 4. Positions taken in the drafting, and why

**ECTA safe harbour.** Chapter XI (ss 70–79) limits liability for service providers hosting third-party content, but the protections are only available to members of a Ministerially-recognised industry representative body that has adopted a code of conduct — in practice ISPA. Separately, the more you moderate, the less passive you look, and passivity is what the hosting limitation assumes. The agreement resolves this by **not claiming the safe harbour as the primary defence** (clause 12.3) and reserving it without prejudice. The real protection is the corroboration threshold. If you do join ISPA, that clause should be revisited — it would then be worth claiming properly.

**Liability.** The original draft excluded personal injury outright. **CPA s51(1)(c)** prohibits a term excluding liability for loss attributable to gross negligence, and s49 requires that risk-limiting terms be conspicuous and separately drawn to the consumer's attention. Clause 15 therefore carves out death, personal injury, gross negligence, intent and fraud; limits what remains; and carries a boxed s49 notice with its own checkbox in Part D. A blanket exclusion would likely have been struck down and would have made the whole clause look bad-faith.

**Whether the CPA applies at all** depends on whether RydrSafe is supplied "in the ordinary course of business for consideration". A free app may fall outside the definition of a transaction. The agreement is drafted as though the CPA applies, which is the safe direction.

**Defamation.** SA common law presumes publication of defamatory matter to be wrongful. The relevant defence here is reasonable publication (*National Media Ltd v Bogoshi* 1998 (4) SA 1196 (SCA)). Every element of clause 6 is aimed at it: corroboration before publication, no free-text narrative, right of reply, prompt take-down, human review. That is what "reasonable publication" looks like in practice.

**POPIA s18.** Driver information is collected from users, not from drivers. s18 requires reasonably practicable steps to notify. Clause 21 relies on a public notice, a driver self-check page, and direct notice before a status change. That is a defensible reading of "reasonably practicable" — but only if the self-check page actually exists (B7).

**POPIA s71.** Nobody flagged this in earlier reviews, and it matters: a risk score that changes a driver's public standing is automated processing affecting a person to a substantial degree. Human sign-off on status changes (B6) is what keeps it inside s71(2).

**Copyright / trade marks.** Real but lower priority. The mitigation is minimisation — delete images after OCR, no operator branding in the RydrSafe UI, nominative reference only. The realistic risk is a cease-and-desist from an operator, not a regulator complaint. Clause 10 covers it in a paragraph, which is proportionate.

**Xenophobia.** Clause 8.6 is deliberate. A community reporting platform aimed at e-hailing drivers in South Africa, with no such clause, will be used to target foreign-national drivers. This needs to be a moderation rule with teeth, not just a line in the terms.

## 5. Placeholders to fill before publishing

Search the agreement for `[` — every bracketed item needs a real value. The ones that need a decision rather than a lookup:

- `[N]` hours for image deletion (clause 25.2) — set it to something you can actually guarantee.
- Retention periods in clause 29 marked `[24]`, `[36]` — these are proposals. Shorter is safer.
- Azure region and backup replication region (clause 27.4) — determines whether s72 transborder rules apply.
- Clause 24.3 status statement — pick one of the three options.
- Verify the Information Regulator's current contact details; they have changed before.

## 6. Suggested order of work

1. Decide the s57 prior-authorisation question with an attorney. Everything else is downstream of it.
2. Write the LIA. It is a day's work and it underpins clause 23.
3. Build B1–B5 (the publication tiers). This is the product change the legal position depends on.
4. Build B9–B10 (consent capture and age gate) — cheap, and nothing can launch without them.
5. Build B6–B8 (human review, driver self-check, appeals).
6. Retention jobs (B12), image hashing (B11).
7. Have an attorney review the agreement *after* the above, so they are reviewing something true.
