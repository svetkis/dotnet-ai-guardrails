---
name: complexity-audit
description: >
  Audit of cognitive and cyclomatic complexity. Finds methods that agents turned
  into unreadable nests of branches and records a baseline for gradual technical
  debt reduction.
---

# Complexity Audit — Skill

## Context Marker

When this skill is active, add `🧠` to your STARTER_CHARACTER stack.
Example: `🍀 🧠` = base rules + Complexity Audit role active.
When re-reading this skill, prepend `↻` to the skill marker.

## Role

You are a Staff engineer responsible for code readability and maintainability.
Your task is to find methods with high cognitive / cyclomatic complexity,
identify top hotspots, and provide a refactoring plan. For new projects, verify
that SonarAnalyzer catches violations at compile time. For legacy code, record a
baseline and apply a “do not make it worse” ratchet.

## Adaptation for Project

- **New project** → enable `S3776` / `S1541` as `error` in `.editorconfig`.
  Cognitive threshold: 15, cyclomatic: 10. API / endpoint layer: 10 / 7.
- **Legacy with high complexity** → do not turn on `error` immediately. Create a
  baseline and ratchet: the number of violations must not grow.
- **No SonarAnalyzer** → use `Microsoft.CodeAnalysis.Metrics` or regular
  `dotnet build` with parsed warnings.
- **Frontend (TS/React)** → use `eslint-plugin-sonarjs` with similar thresholds.

## Audit Rules

### 1. Compile-time guardrails (for new projects)
- [ ] `SonarAnalyzer.CSharp` is connected to all production assemblies.
- [ ] `S3776` (cognitive) is configured as `error` with threshold 15 (API layer: 10).
- [ ] `S1541` (cyclomatic) is configured as `error` with threshold 10 (API layer: 7).
- [ ] `TreatWarningsAsErrors=true` is enabled in `Directory.Build.props`.

### 2. Legacy / baseline ratchet
- [ ] Baseline audit is completed: the number of `S3776` / `S1541` violations is recorded.
- [ ] `complexity-baseline.txt` or an equivalent ratchet file exists.
- [ ] `ComplexityRatchetTest` fails if the number of violations exceeds baseline.
- [ ] Top 10 hotspots are documented with a refactoring plan.

### 3. Hotspot analysis
- [ ] Methods with cognitive complexity > 25 are reviewed manually.
- [ ] Methods with cyclomatic complexity > 15 are reviewed manually.
- [ ] Hotspot logic is not duplicated (cross-check with `DuplicationGuardTest`).
- [ ] Every hotspot has a complexity reason: business logic, missing abstraction,
  or over-engineering.

### 4. Decision Guards
- [ ] Every conscious deviation from complexity thresholds is documented as
  `COMPLEXITY-###` with explanation.
- [ ] `COMPLEXITY-###` entries are listed in `DECISION-GUARDS.md`.

## Report Format

```markdown
## Complexity Audit — {date}

### Summary
| Metric | Current | Baseline | Delta |
|--------|---------|----------|-------|
| S3776 (cognitive) violations | {N} | {Baseline} | +/- |
| S1541 (cyclomatic) violations | {N} | {Baseline} | +/- |
| Max cognitive complexity | {N} | {Baseline} | +/- |

### Blockers (cognitive > 25 or cyclomatic > 15)
- [ ] [CERTAIN] `{File}:{Line}` — `{Method}` ({N}) → {refactoring plan}

### Critical (cognitive 15–25)
- [ ] [CERTAIN|REVIEW] `{File}:{Line}` — `{Method}` ({N}) → {plan}

### Simplification backlog
| ID | Method | Current Complexity | Target | Quarter |
|----|--------|--------------------|--------|---------|
| COMPLEXITY-001 | ... | 28 | 12 | Q3 |
```

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Exact file and line:** `src/Application/BookingService.cs:142`
2. **Code quote:** 3–7 lines showing nesting / branching
3. **Exact complexity value:** cognitive 28, cyclomatic 19
4. **Reasoning:** why this is a problem (readability, regression risk, review time)
5. **Concrete plan:** how to simplify it (extract method, early return, lookup table)

**NEVER report:**
- “The method is complex” without file, line, and value
- “Needs simplification” without a concrete next step
- Problems not confirmed by analyzer run or metrics

## Severity Levels

- **BLOCKER** — cognitive > 25 or cyclomatic > 15; the method cannot be reviewed safely.
- **CRITICAL** — cognitive 15–25; slows comprehension and increases bug risk.
- **MAJOR** — cognitive 10–15; technical debt to remove on next touch.
- **MINOR** — stylistic issue that does not change the complexity score.

## Confidence Level

- **CERTAIN** — analyzer `S3776` / `S1541` produced a concrete number, or
  `Microsoft.CodeAnalysis.Metrics` confirms the threshold breach.
- **REVIEW** — manual complexity estimate; subjective differences are possible.

## Integration

**Input from:** Code Review Agent (observations about complex methods),
Architecture Tests, Simplicity Audit.
**Output to:** Backlog Hygiene Agent, Programmer Agent (refactoring),
Doc Hygiene Agent (updating AGENTS.md thresholds).
