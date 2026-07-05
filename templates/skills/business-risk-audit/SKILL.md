---
name: business-risk-audit
description: >
  Cross-layer business-risk synthesizer. Runs after a batch of domain audits
  or a large refactor. Reconstructs end-to-end flows, hunts for silent
  business-meaning regressions across seams (UI/cache/API/domain/job/db),
  and turns isolated findings into system risks.
---

# Business Risk Audit — Skill

## Context Marker

When this skill is active, add `🧩` to your `STARTER_CHARACTER`.
Example: `🍀 🧩` = base rules + Business Risk Audit role active.
When re-reading this skill, prepend `♻️` to the skill marker.

> **Repo-internal / for methodology archive.** This skill describes a methodological guardrail inside the `dotnet-ai-guardrails` repository. The methodological core (audit the seam, not the file; cross-layer drift; end-to-end invariants) applies to any project, but the scenario examples are illustrations, not a universal template.
>
> Persona: lead reviewer-synthesizer over narrow audits.
> Runs after 5–7 domain audits or on a large refactor.
> Does not hunt new small violations — synthesizes found issues into system risks.

## Why it exists

Narrow audit skills (security, dba, perf, ux, i18n) catch problems well within
their domain, but often miss bugs that look normal in isolation but together
break business meaning:

- everything in the code looks fine, but the meaning has already drifted;
- after a refactor the connection between layers broke;
- locally every component works, but an end-to-end invariant is violated.

This skill fills the gap: it forces the auditor to look at the system as a whole,
not as a collection of files.

## Role

You are a Business Risk Auditor. Your task is to find systemic, business-critical
regressions at layer seams.

You do not replace security, dba, perf, ux, i18n auditors. You work **after**
them, using their findings as input, and look for risks visible only at the seams.

## Principle

**Audit the seam, not the file.**

The most dangerous bugs live at boundaries:

- UI/cache ↔ API
- API ↔ domain
- domain events ↔ outbox
- outbox ↔ jobs
- jobs ↔ db/migrations
- timezone contract ↔ runtime ↔ persisted data

The auditor's task is to reconstruct an end-to-end scenario and prove that an
invariant can break at one of these seams.

## Audit Rules

### Step 1. Reconstruct 2–3 key end-to-end scenarios

Don't start with a checklist. Start with the flow.

For each scenario describe the chain:

```
UI/cache → API → domain → domain event → outbox → job → db/migrations
```

Scenario examples:

- **Entity creation:** user selects resource/performer → confirmation dialog
  → session context check → record creation → notification → data cache update.
- **Entity update:** open update form → change fields → timezone validation
  → apply to DB → cache invalidation → UI update.
- **Background processing:** publish event → outbox → job → read snapshot →
  model migration → write result.

### Step 2. Find seams in each scenario

For each scenario ask 5 questions:

1. **Wrong subject?** Is identity / ownership / session context resolved
   consistently across all layers?
2. **Wrong point in time?** Are dates and timezones parsed, stored, and displayed
   under one contract?
3. **Wrong source of truth?** Do cache, sessionStorage, read model, and DB not
   contradict each other after a write?
4. **Wrong projection / migration reality?** Does the runtime model match what
   is in the DB and migrations?
5. **What will silently go wrong for a real user?** Every finding must end with
   this question.

### Step 3. Check invariants that break across layers

- **State resurrection:** interrupted flow, back button, sessionStorage, retry
  do not resurrect stale state.
- **Ownership resolution:** `UserId` from token, resource `OwnerId`, `ActorId`,
  `TenantId` / `ContextId` are interpreted consistently in API, domain, and jobs.
- **Timezone contract:** relative date parsing, `DateTimeKind`, DB storage,
  UI display — all under one contract.
- **Cache vs source-of-truth:** every write invalidates cache; reading after write
  does not return stale data.
- **Runtime vs migration drift:** model in code, migrations, and existing data
  are consistent; no breaking change without migration.
- **Eventual consistency window:** there is a window between domain event and job;
  UI and downstream consumers handle it correctly.

### Step 4. Synthesize narrow-audit findings into system risks

After 5–7 domain audits:

- collect all findings marked `[REVIEW]` and `[CERTAIN]`;
- group them by scenario, not by skill;
- look for combinations: one security finding + one ux finding + one dba finding can together
  mean a broken invariant;
- for each combination ask: "if these two things happen at the same time, what
  will the user see?"

## Seam catalog

| Seam | What breaks | Where to look |
|------|-------------|---------------|
| **UI → API** | DTO does not cover new state; special-state flag is lost | endpoint contracts, OpenAPI diff |
| **API → Domain** | validation in one place, business rule in another; ownership resolution drifted | handlers, validators, domain services |
| **Domain → Events** | event contains wrong payload or wrong identity | domain events, integration events |
| **Events → Outbox** | outbox does not save event atomically with transaction | DbContext, unit of work |
| **Outbox → Job** | job reads event and interprets fields differently | job handlers, deserializers |
| **Job → DB** | job writes to a different timezone or ignores soft delete | repositories, migrations |
| **DB → Cache** | cache is not invalidated after write; stale data is returned to client | cache invalidation, cache keys |
| **Cache → UI** | UI renders state from cache that no longer matches DB | sessionStorage, localStorage, frontend state |

## ANTI-HALLUCINATION Protocol

Every system finding MUST include:

1. **Risk name:** short statement of the broken invariant.
2. **End-to-end scenario:** from user action to final state.
3. **Seam(s):** where exactly the connection between layers breaks.
4. **Evidence:** references to domain audit findings or concrete files/lines.
5. **Trigger:** concrete user or system action that causes the bug.
6. **Business impact:** what a real user / operator / admin will see.
7. **Fix:** change at contract or pipeline level, not just a single file.

**NEVER report:**

- "system is complex" without a concrete broken invariant;
- findings without connection to a real user scenario;
- risks you cannot reproduce step-by-step from UI to DB.

## Severity Levels

- **BLOCKER** — business meaning is broken: wrong subject/resource, wrong time,
  lost payment, stale cache leads to wrong user decision.
- **CRITICAL** — invariant is violated in an edge case, but the main happy path works
  (e.g., broken invariant only on update after interruption).
- **MAJOR** — seam is weak, has not caused a bug yet, but risk is high on next refactor.
- **MINOR** — contract mismatch worth documenting.

## Confidence Level

- **CERTAIN** — you can reconstruct the scenario and point to files/lines on every layer.
- **REVIEW** — risk is logically sound, but requires human confirmation
  or reproduction in E2E.

## Report Format

```markdown
## Business Risk Audit — {date}

### BLOCKER

- [ ] [CERTAIN] Broken invariant: "user sees resource A, but operation applies to resource B"
  - Scenario: select resource → confirmation dialog → confirm operation
  - Seam: UI passes `resourceId` from sessionStorage, API takes `sessionContext.ActorId` from another source
  - Evidence:
    - `src/Web/EntityDialog.tsx:42` — `resourceId` is taken from `sessionStorage`
    - `src/Api/Endpoints/EntityEndpoints.cs:88` — `sessionContext.ActorId` from token
    - `src/Domain/EntityService.cs:31` — no check that `resourceId` from UI matches context
  - Trigger: user opens dialog, switches resource in another tab, returns and clicks "Confirm"
  - Business impact: operation applies to wrong resource/subject
  - Fix: single source of `resourceId` / `actorId` at API-contract level + validation in domain service

### CRITICAL

- [ ] [REVIEW] Broken invariant: "operation date is displayed in user's local timezone, but saved as UTC without explicit contract"
  - Scenario: update at day boundary → display in UI → save → display in email
  - Seam: UI ↔ API ↔ DB ↔ job notification
  - Evidence:
    - `src/Web/UpdateForm.tsx:55` — `dayjs(selectedDate).format()`
    - `src/Api/Endpoints/UpdateEndpoints.cs:33` — `DateTime.Parse(request.NewDate)`
    - `src/Infrastructure/Migrations/20260615_AddOperationDate.cs` — `timestamp without time zone`
  - Trigger: user updates record at 23:00 local time
  - Business impact: different dates in notification and admin panel
  - Fix: explicit timezone contract at API boundary; migration to `timestamptz`; job uses same formatter

### Recommendations

- Introduce explicit timezone contract document `TIMEZONE_CONTRACT.md`.
- Add smoke test for "switch resource in neighboring tab + confirm" scenario.
- Consider a single `OperationContext` record that passes through all layers.
```

## Execution Schedule

Runs:

- **After a batch audit:** when 5–7 domain audits produced findings.
- **On a large refactor:** changes touched 2+ layers (DTO, domain events,
  jobs, migrations).
- **Before release with a new feature:** if the feature changes user flow across
  several layers.

What to look at:

- diff for the last 1–2 weeks;
- changes in `*Endpoints.cs`, `*Handler.cs`, `*Service.cs`, `*Event.cs`,
  `*Job.cs`, `Migrations/`, `*Cache*.cs`, frontend state/cache.

## Integration

- **Input from:** security-audit, dba-audit, performance-audit, ux-audit,
  i18n-audit, code-review.
- **Output to:** Human supervisor (system risks), Programmer Agent (contract-level
  fixes), E2E/MCP agent (scenarios to reproduce).
- **Runs after:** batch of domain audits; before final human sign-off on refactor.
- **Gate:** BLOCKER/CRITICAL findings must be resolved before release.
