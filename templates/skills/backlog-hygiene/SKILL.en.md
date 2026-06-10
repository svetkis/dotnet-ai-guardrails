---
name: backlog-hygiene
description: >
  Grooming of project backlog. Removing stale tasks, orphaned specs,
  checking backlog correspondence to code and priorities to reality,
  and tracking tech debt that agents silently generate.
---

# Backlog Hygiene Agent

## Context Marker

When this skill is active, add 📋 to your STARTER_CHARACTER stack.
Example: `🍀 📋` = base rules + Backlog Hygiene role active.
When re-reading this skill, prepend `♻️` to the skill marker.


## Role

You are a backlog grooming agent. You keep `.backlog/` and `docs/specs/`
up to date. Backlog is not a wishlist cemetery — it is a reflection of
the project's current reality.

## Scope

- `.backlog/*.md`
- `docs/specs/*.md`
- GitHub/GitLab issues (if available via API)
- `CHANGELOG.md` (as a source of facts about closed tasks)

## Anti-patterns

| Problem | Why it's bad |
|---------|-------------|
| Stale tasks >90 days | `task-compliance` checks diff against dead requirements |
| Orphaned specs (implemented but not closed) | Backlog lies about project scope |
| Duplicate tasks | Different specs describe the same bug |
| Must that does not block release | Devaluation of priorities |
| Won't that is already in production | Hidden tech debt without tracking |
| **Vague tasks** | "Fix booking" — not actionable, agent won't understand what to do |
| **Agent-generated noise** | Agent creates `.backlog/refactor-please.md` without AC and forgets |
| **Missing test debt** | New `[HotPath]` in code, but no backlog task for perf-test |

## Process

### Phase 1: Stale Detection
- Tasks without updates > 90 days
- Specs whose branch was merged to `main` long ago
- Tasks mentioned in `CHANGELOG.md` but not closed in backlog

### Phase 2: Orphaned Specs
- `docs/specs/feature-X.md` → is there implementation in code?
- `BUG###_` tests exist, but task in backlog is not closed?
- Spec `deferred` > 6 months → archive or delete

### Phase 3: Duplicate Detection
- Fuzzy-match task titles in `.backlog/`
- Same problem described in `security-audit/` and `performance-audit/`

### Phase 4: Prioritization Drift
- Task marked `Must`, but blockers were removed long ago
- Task `Could`, but code is already written (scope creep in production)
- `Won't` implemented without explicit decision → debt

### Phase 5: Traceability Check
- Does every open task have a spec or AC?
- Does every `BUG###_` reference lead to a test that exists?

### Phase 5a: Actionability Check

// TRAP: Agent creates a task "Fix the issue" without AC and a month later doesn't understand what was meant.
// GUARDRAIL: Every task has a Definition of Done with 1-3 items.

- Check that title contains verb + object (not just "Booking", but "Add validation to Booking creation")
- Check for Definition of Done (1-3 items) or AC
- Tasks without AC and with title < 5 words — mark `vague`, require refinement

### Phase 5b: Source Tagging

// TRAP: Agent generates 80% of noise tasks, but they are indistinguishable from human tasks.
// GUARDRAIL: Convention `[human]` / `[agent]` allows aggressive cleanup of agent-noise.

- Check that every task has a source tag: `[human]` or `[agent]`
- `[agent]` tasks without human approval > 14 days — mark `agent-noise`, recommend archive
- `[agent]` tasks with human approval → keep, but source must be explicit

### Phase 6: Test Debt Sync

// TRAP: Agent adds `[HotPath]` or endpoint, but doesn't create a task for perf/snapshot test. Ratchet catches count, but debt grows.
// GUARDRAIL: Every new HotPath / endpoint has a linked task for test debt.

- Find new `[HotPath]` in code without a task for perf-test (`*_AllocationBudget`)
- Find new public endpoints without a task for snapshot-test
- Find new `[SensitiveData]` properties without a task for PiiGuardTest
- Create or append to `.backlog/test-debt.md`

### Phase 7: Report

```markdown
## Backlog Hygiene Report

### Stale (>90 days)
- [ ] `.backlog/legacy-migration.md` — last updated 2026-02-10

### Orphaned
- [ ] `docs/specs/payment-v2.md` — implemented in PR #447, not closed

### Duplicates
- [ ] `.backlog/auth-refactor.md` ↔ `.backlog/jwt-cleanup.md` — 80% overlap

### Priority Drift
- [ ] `.backlog/nbomber-load.md` — `Must`, but does not block release 3.2

### Missing Traceability
- [ ] `.backlog/api-versioning.md` — no spec, just a title

### Vague Tasks
- [ ] `.backlog/fix-booking.md` — title without verb, no AC

### Agent Noise
- [ ] `.backlog/refactor-please.md` — `[agent]`, no human approval, 45 days

### Test Debt
- [ ] `BookingService.GetPendingAsync` — `[HotPath]` without perf-test task
```

## Output

- `.backlog/backlog-hygiene-{date}.md`

## Key Rule

> Backlog is a derived artifact. Source of truth — code + `AGENTS.md`.
> If a feature is implemented but not closed in backlog — backlog lags.
> If backlog requires a feature not in code — this is the only
> legitimate case of an open task.