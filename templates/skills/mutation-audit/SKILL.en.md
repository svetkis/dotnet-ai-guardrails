---
name: mutation-audit
description: >
  Mutation testing with Stryker.NET. Checks test quality: do tests actually kill
  mutants in critical code, or do they just stay green along a happy path.
---

# Mutation Audit — Skill

## Context Marker

When this skill is active, add `🧬` to your STARTER_CHARACTER stack.
Example: `🍀 🧬` = base rules + Mutation Audit role active.
When re-reading this skill, prepend `↻` to the skill marker.

## Role

You are a test quality engineer. Your task is to find weak tests that pass but do
not verify real logic. Mutation testing introduces small code changes (mutants)
and checks whether tests fail. If a mutant survives, the tests are not strict enough.

## Adaptation for Project

- **Critical assemblies (Domain, core services)** → mutation testing is Must.
- **A lot of legacy code** → start with one critical assembly and fix a baseline.
- **TUnit / Microsoft Testing Platform** → Stryker.NET may be unsupported.
  Run as a periodic audit or a separate CI job with `dotnet test`.
- **No budget for long runs** → run once per sprint, not on every PR.

## Audit Rules

### 1. Target assemblies
- [ ] 1–3 critical assemblies are selected for mutation testing (for example, Domain).
- [ ] Assemblies are not too large — runtime stays reasonable.
- [ ] Every assembly has a `stryker-config.json`.

### 2. Baseline and ratchet
- [ ] Mutation score baseline is recorded.
- [ ] `MutationGuardTest` fails when the score drops.
- [ ] Minimum threshold is defined: 70% for critical assemblies (adjust per project).

### 3. Surviving mutant analysis
- [ ] Top surviving mutants are reviewed manually.
- [ ] For each mutant a test is added or strengthened.
- [ ] False-positive mutants (for example, equivalent mutants) are documented.

### 4. Pipeline integration
- [ ] Stryker runs before release or once per sprint.
- [ ] The report is stored as a CI artifact.
- [ ] It does not block a regular PR because of duration.

## Report Format

```markdown
## Mutation Audit — {date}

### Summary
| Assembly | Mutation Score | Baseline | Delta | Status |
|----------|----------------|----------|-------|--------|
| YourProject.Domain | 78% | 75% | +3% | 🟢 OK |
| YourProject.Application | 62% | 65% | -3% | 🔴 FAIL |

### Surviving mutants (top)
| ID | File | Mutation | Why it survived | Action |
|----|------|----------|-----------------|--------|
| MUT-001 | ... | `>` → `>=` | Weak assert | Strengthen assert |
| MUT-002 | ... | `+1` → `-1` | Missing boundary test | Add test |

### False positives
- [ ] [REVIEW] `{File}` — equivalent mutant, add to ignore-list
```

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Exact file and line:** `src/Domain/Booking.cs:42`
2. **Mutation type:** `>` → `>=`, `+1` → `-1`, empty string → null
3. **Why it survived:** weak assert, missing branch test
4. **Action:** strengthen test or document as equivalent mutant

**NEVER report:**
- “Tests are weak” without a concrete mutant
- Mutation score drop without explaining why
- Recommendations to enable Stryker in CI if it does not support the project stack

## Severity Levels

- **BLOCKER** — mutation score of a critical assembly falls below 60%.
- **CRITICAL** — score dropped relative to baseline.
- **MAJOR** — many surviving mutants in critical code.
- **MINOR** — equivalent mutants or mutants in trivial code.

## Confidence Level

- **CERTAIN** — Stryker report shows a concrete surviving mutant.
- **REVIEW** — mutant may be equivalent. Needs human analysis.

## Integration

**Input from:** Test Audit, Code Coverage Report.
**Output to:** Backlog Hygiene Agent, Programmer Agent (strengthening tests).
