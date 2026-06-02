# Human Audit Bridge — Manual Audit with AI Checklists

> **Purpose:** Use existing AI skills for **manual** project audit.  
> **Consumer:** Human (Tech Lead, Senior Dev, auditor).  
> **Time:** 1–2 hours per audit skill.  

---

## Problem

All skills in `skills/` are written as **agent roles** ("You are a Security auditor…").  
But every skill contains `CHECKLIST.md` — a structured checklist that works great in human hands.  
This document is a "bridge": how to take an AI artifact and use it for a manual audit.

---

## Map: Which Audit — Which Checklist

| I want to check… | Take CHECKLIST from… | Time | When to run |
|-------------------|----------------------|-------|-----------------|
| Data leaks, authorization, input | [`skills/security-audit/CHECKLIST.md`](../../skills/security-audit/CHECKLIST.md) | 1–2 h | Before release / once per sprint |
| DB queries, migrations, indexes | [`skills/dba-audit/CHECKLIST.md`](../../skills/dba-audit/CHECKLIST.md) | 1–2 h | During migrations / once per sprint |
| Performance, cache, N+1 | [`skills/performance-audit/CHECKLIST.md`](../../skills/performance-audit/CHECKLIST.md) | 1–2 h | Before release |
| API contracts, DTO, OpenAPI | [`skills/api-design-audit/CHECKLIST.md`](../../skills/api-design-audit/CHECKLIST.md) | 1 h | Once per sprint |
| Duplication, dead code, drift | [`skills/tech-debt-audit/CHECKLIST.md`](../../skills/tech-debt-audit/CHECKLIST.md) | 1–2 h | Before quarterly planning |
| Test coverage, dead tests | [`skills/test-audit/CHECKLIST.md`](../../skills/test-audit/CHECKLIST.md) | 1 h | After 3–5 features |
| UX, accessibility, mobile layout | [`skills/ux-audit/CHECKLIST.md`](../../skills/ux-audit/CHECKLIST.md) | 1 h | Before beta |
| Localization, date/number formats | [`skills/i18n-audit/CHECKLIST.md`](../../skills/i18n-audit/CHECKLIST.md) | 30 min | Once per sprint |
| SDK, NuGet, dependency versions | [`skills/version-audit/CHECKLIST.md`](../../skills/version-audit/CHECKLIST.md) | 30 min | Once per sprint |
| Task compliance (scope creep) | [`skills/task-compliance/CHECKLIST.md`](../../skills/task-compliance/CHECKLIST.md) | 15 min | Per PR |

---

## How to Read an AI Skill as a Human

An AI skill consists of 4 sections. A human only needs 2:

| Section | Needed by human? | What to do |
|--------|----------------|------------|
| `## Role` | ❌ No | Ignore. This is a prompt for the agent. |
| `## Project Adaptation` | ✅ Yes | Read and cross out items not applicable to your stack. |
| `## Audit Rules` | ✅ Yes | This is your checklist. Go through the items. |
| `## Report Format` | ⚠️ Partially | Use severity (Critical / Medium), but don't copy the markdown template verbatim. |

**Process:**

1. **Open** `CHECKLIST.md` (not `SKILL.md`)
2. **Read** `ADAPTATION.md` — cross out what's not applicable to your stack
3. **Go through checkboxes** — mark `[x]` or `[-]` (N/A)
4. **Record findings** — in a file or in the backlog

---

## Manual Audit Process (Step by Step)

### Step 0. Preparation (5 min)

- Identify project stack (`.NET`, `EF/Dapper`, `Minimal API/MVC`, `Clean/VSlice`)
- Open `skills/ADAPTATION.md` — find your configuration in the table
- Pick one audit — don't try to cover everything at once

### Step 1. Checklist Adaptation (10 min)

Go through `CHECKLIST.md` and mark:
- `[ ]` — applicable, will check
- `[-]` — not applicable to our stack (e.g., EF rules in a Dapper project)
- `[?]` — not sure, will defer

### Step 2. Code Walkthrough (main time)

For each checklist item:
1. Find the corresponding code (`grep`, IDE search, or structural search)
2. Check the fact — don't trust comments
3. If you found a violation — record it:
   - File and line
   - Code quote (3–5 lines)
   - Why this is a violation (which rule)
   - What to do

### Step 3. Classification (10 min)

Sort findings by severity:

| Severity | What it is | Action |
|----------|---------|----------|
| **BLOCKER** | Breaks prod or security | Task immediately, hotfix if in prod |
| **MAJOR** | Perf / correctness degradation | Task in current sprint |
| **MINOR** | Suboptimal, tech debt | Task in backlog |
| **REVIEW** | Needs human judgment | Discuss with the team |

### Step 4. Recording (10 min)

Create tasks in the backlog. Each finding is one task if the fix takes > 15 min.  
If there are many findings — group by type.

---

## Report Format for Humans

No need to copy AI templates verbatim. Use a short format:

```markdown
## Manual Audit: {Name} — {date}

### Audited by
@username

### Scope
{which files / modules were checked}

### Critical (BLOCKER)
- [ ] {description} → `{file:line}`
  - Code: `{quote}`
  - Why: {rule from checklist}
  - What to do: {specific action}

### Medium (MAJOR)
- [ ] {description} → `{file:line}`

### Minor (MINOR)
- {description}

### What was not applicable
- {checklist item} — reason: {why N/A}

### Recommendations
- {general conclusions, not tied to lines}
```

---

## Example: Manual Security Audit

**Stack:** .NET 10, Minimal API, EF Core, PostgreSQL  
**Checklist:** `skills/security-audit/CHECKLIST.md`

**Adaptation:**
- ❌ Checking `[Authorize]` on controllers — not applicable (Minimal API)
- ✅ Checking `.RequireAuthorization()` — we replace
- ✅ Checking PII in logs — keep

**Walkthrough:**
```markdown
### Critical
- [ ] PII in logs: `UserEmail` written to Info log → `src/Api/Endpoints/BookingEndpoints.cs:42`
  - Code: `logger.LogInformation("Booking created for {Email}", request.Email);`
  - Why: Security-audit → Data Exposure → logs: no PII
  - What to do: Replace with `{UserId}` or redact via `IEmailRedactor`

### Medium
- [ ] Endpoint `/api/webhooks/payment` without `.RequireAuthorization()` → `src/Api/Endpoints/PaymentEndpoints.cs:15`
  - Code: `app.MapPost("/api/webhooks/payment", HandlePayment);`
  - Why: No explicit webhook protection
  - What to do: Verify `X-Secret-Token` in middleware or add `.RequireAuthorization()` + policy
```

---

## Efficiency Tips

| Don't | Do |
|----------|-------|
| Go through all checklists in one day | Take one audit per sprint |
| Trust comments in code | Read code and check the fact |
| Create tasks "fix everything" | One finding = one task |
| Ignore `[REVIEW]` findings | Discuss them at retro |
| Rewrite CHECKLIST.md | Mark `[-]` and move on |

---

## Integration with AI Audit

Manual audit is not a replacement for AI skills, but a **complement**:

- **AI audit** — good for volume: go through 50 files in 10 minutes, find obvious patterns.
- **Human audit** — needed for context: "this endpoint without authorization is OK, it's public by design."

**Hybrid approach:**
1. Run the AI skill — get a draft of findings
2. Go through CHECKLIST.md manually — filter out false positives
3. Add findings that the AI doesn't see (business context, implicit side effects)

---

> **Principle:** Checklists in this repository are not the property of agents. They are proven checklists. A human who uses them gets the same result, only with better judgment.
