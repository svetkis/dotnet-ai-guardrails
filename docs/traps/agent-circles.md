# Trap: Agent Circles

## Scenario

The agent enters a fix → fix → fix (or fix → revert) loop when an optimization or refactoring touches too many subsystems. The agent sees a local problem, fixes it, but creates a new one — and so on in circles. Sometimes the only way out is to revert the entire change.

### Examples from Practice

**1. NoTracking default — 21 hours in production**
```
perf: QueryTrackingBehavior.NoTracking globally (p99: 2727→209ms, 13x!)
  → fix: AsTracking() in 5 write-methods (patch)
  → db: remove NoTracking default, manual AsNoTracking (proper fix)
```

**2. Tailwind 3→4 — 5 fixes in 20 hours, then full revert**
```
upgrade Tailwind CSS 3→4
  → fix: outline-none, bg-opacity, ring-offset breaking
  → fix: CSS reset cascade layers
  → fix: Docker rolldown musl binding
  → REVERT: CSS reset fundamentally incompatible
```

**3. .Include() → .Select() — 11 files refactored, 46 files fixed**
```
refactor: replace Include/FirstOrDefaultAsync with Select projections (11 files)
  → fix: "Fixed all entity pass-throughs" (46 files!)
  → 4 additional perf commits with follow-ups
```

## Why the Agent Enters the Loop

1. **Doesn't see blast radius** — optimizes service A, not knowing that service B depends on A's side effect
2. **Tests give false confidence** — InMemory DB, isolated mocks, no cross-service tests
3. **Fixes symptom, not cause** — `AsTracking()` in 5 methods instead of reverting global NoTracking
4. **Visual bugs are invisible** — `tsc --noEmit` passes, but layout is broken
5. **Each fix creates an illusion of progress** — "one more commit and it's done"

## Signs of Entering the Loop

- Second fix-commit for the same problem
- Fix touches files that weren't in the original change
- Commit message contains "still", "another", "properly", "actually"

## Solution

### When to Interrupt

| Signal | Action |
|--------|--------|
| 2nd fix-commit for the same problem | Review blast radius |
| 3rd fix-commit | Consider revert |
| Fix touches 4x more files than the original | **Definitely revert** |

### Prevention

1. **E2E testing after perf commits** — catches 80% of loops (stale cache, layout breaks)
2. **Integration tests instead of mocks** — for cross-service interactions
3. **Rule: after an agent's perf commit — manual audit of write-paths**
4. **Ratchet tests** — prevent removal of critical attributes during refactoring

## Pattern

See `tests/patterns/RatchetTest.cs` and `tests/patterns/ArchitectureRules.cs`
