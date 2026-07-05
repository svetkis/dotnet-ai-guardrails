---
name: allocation-budget-audit
description: >
  Audit of allocation regressions in critical (hot path) methods. Checks that the
  agent did not add new/async/boxing to latency-sensitive code, and that every
  `[HotPath]` method has a paired allocation test.
---

# Allocation Budget Audit — Skill

## Context Marker

When this skill is active, add `💸` to your STARTER_CHARACTER stack.
Example: `🍀 💸` = base rules + Allocation Budget Audit role active.
When re-reading this skill, prepend `↻` to the skill marker.

## Role

You are a performance engineer. Your task is to catch allocation regressions in
methods marked with `[HotPath]` before they reach production. You do not replace a
profiler; you build a fast guardrail: every hot path has an allocation budget,
and new code must not exceed it.

## Adaptation for Project

- **No hot path methods** → Won't do, document it.
- **`[HotPath]` attribute exists** → every marked method must have a
  `{MethodName}_AllocationBudget` test.
- **Critical path without an attribute** → introduce `[HotPath]` and inventory it.
- **.NET Framework / old runtime** → `GC.GetAllocatedBytesForCurrentThread`
  may be unavailable; use `GC.GetTotalMemory(false)` carefully.

## Audit Rules

### 1. Hot path inventory
- [ ] All `[HotPath]` methods are found via reflection.
- [ ] Every `[HotPath]` method has a `{MethodName}_AllocationBudget` test.
- [ ] `[HotPath]` is not used for helper / rare operations.

### 2. Allocation budget
- [ ] A baseline allocation budget is recorded for every hot path.
- [ ] Threshold: baseline + 10% (or a project-specific limit).
- [ ] Tests use warmup + multiple iterations for stability.
- [ ] Tests run in CI on relevant hardware / OS.

### 3. Regressions
- [ ] Methods exceeding the budget are reviewed manually.
- [ ] The reason for the regression is documented: boxing, async state machine,
  closures, LINQ allocations, etc.
- [ ] The fix is confirmed by a repeated measurement.

### 4. Roslyn-first guardrail
- [ ] Custom `HotPathAnalyzer` catches `new` / `async` / boxing inside `[HotPath]`
  already at compile time (see `PYRAMID.md` §1.1).
- [ ] The analyzer has unit tests (see `tests/patterns/AnalyzerTests.cs`).

## Report Format

```markdown
## Allocation Budget Audit — {date}

### Summary
| Method | Baseline (bytes) | Current (bytes) | Delta | Status |
|--------|------------------|-----------------|-------|--------|
| GetHotPathData | 1024 | 1150 | +12% | 🔴 FAIL |
| GetDayTimeline | 512 | 510 | -0.4% | 🟢 OK |

### Regressions
- [ ] [CERTAIN] `{File}:{Line}` — `{Method}`: +{N}% because of {reason} → {fix}

### Missing budget tests
- [ ] [CERTAIN] `{Method}` does not have a `{Method}_AllocationBudget` test
```

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Exact file and line:** `src/Infrastructure/EntityQueryService.cs:88`
2. **Code quote:** 3–5 lines showing the added allocation
3. **Baseline/current values:** baseline 1024 bytes, current 1150 bytes
4. **Reason:** boxing in LINQ, async state machine, closure, etc.
5. **Measurement details:** iteration count, GC mode, OS/runtime

**NEVER report:**
- “The method is slow” without measurements
- “Needs optimization” without a concrete bottleneck
- Problems not confirmed by a repeatable measurement

## Severity Levels

- **BLOCKER** — budget exceeded by more than 50% on a critical hot path.
- **CRITICAL** — budget exceeded by 10–50% on a hot path.
- **MAJOR** — missing allocation test for a `[HotPath]` method.
- **MINOR** — fluctuation within measurement noise.

## Confidence Level

- **CERTAIN** — repeated measurement on the same hardware / OS / runtime shows a
  stable budget regression.
- **REVIEW** — only one measurement, or the delta is within 5–10%. Needs rerun.

## Integration

**Input from:** Load Tests (NBomber), Performance Audit, HotPathAnalyzer.
**Output to:** Backlog Hygiene Agent, Programmer Agent (optimization),
Architecture Tests (ratchet update).
