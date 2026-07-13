---
name: analyzer-tests-audit
description: >
  Audit of unit tests for custom static analyzers. Checks that custom guardrails
  (diagnostics, linters) have positive/negative cases and do not silently break
  after analyzer updates.
---

# Analyzer Tests Audit — Skill

> Optional interaction convention (agent-specific): some agents mark an active
> skill with an emoji in their status stack (e.g., `🔬` for this skill, prefixed
> with `♻️` on re-read). The skill is fully usable without it.

## Purpose and Non-Goals

You are an engineer for compiler-level guardrails. Your task is to make sure the
project's custom static analyzers are tested: they trigger where they should
(positive cases) and do not trigger where they should not (negative cases). This
prevents silent guardrail breakage after analyzer package updates.

Portable core:
- Every custom static analyzer in the project must be covered by positive and negative tests.
- Tests must verify not only that the diagnostic fires, but also the accuracy of its location/span.
- Updates to analyzer dependencies (Roslyn, ESLint, etc.) must be accompanied by analyzer test runs.

Non-goals: auditing third-party analyzers, writing the analyzers themselves,
judging whether a rule is a good idea — only whether it is tested.

## Applicability and Exclusions

Requires adaptation:
- Analyzer technology: Roslyn, ESLint, custom linter, AST script.
- Project diagnostic / rule IDs.
- Test framework and method for attaching reference assemblies / test fixtures.

Adaptation for project:
- **No custom analyzers** → Won't do, document it.
- **Custom analyzers exist** → every diagnostic / rule ID must have positive + negative tests.
- **Analyzers without tests** → high priority, create tests.
- **Only third-party analyzers are used** → verify that configuration
  is covered by tests (for example, ratchet for banned symbols / suppressed rules).

Not applicable when:
- The project has no custom analyzers / linters.
- Only third-party analyzers are used without project-specific rules.

## Required Inputs

- Repo access: analyzer sources, their test projects, and CI configuration.
- The inventory of custom diagnostic / rule IDs (or the means to build it).
- The analyzer dependency versions currently in use (to spot untested updates).

## Procedure

### 1. Analyzer coverage
- [ ] Every diagnostic / rule ID has at least one positive test.
- [ ] Every diagnostic / rule ID has at least one negative test.
- [ ] Analyzers with whitelists / exceptions have exception tests.
- [ ] Analyzers with configurable parameters are tested with multiple configurations.

### 2. Test quality
- [ ] Tests verify the exact diagnostic location (span / line / column).
- [ ] Tests use reference fixtures / assemblies matching the target runtime.
- [ ] Code fix providers / autofixes (if any) are covered by tests.

### 3. Regression guard
- [ ] Analyzer tests run in CI on project build.
- [ ] Updates of analyzer packages are accompanied by analyzer test runs.

### 4. Inventory
- [ ] The list of custom analyzers and diagnostic / rule IDs is documented.
- [ ] For each ID, the docs explain what it catches, why it exists, and which test covers it.

### Project-specific examples

> The examples below illustrate application in .NET + Roslyn. Replace with your analyzer stack.

Example: .NET + Roslyn analyzers

- **Diagnostic IDs:** `SAE001`, `SAE002`, etc.
- **Reference assemblies:** tests use `ReferenceAssemblies` matching the target .NET version.
- **Test template:** see `tests/patterns/AnalyzerTests.cs` in the methodology repository.

## Evidence Requirements

Every finding MUST include:
1. **Diagnostic / rule ID:** `SAE001` / `no-floating-promises`
2. **Analyzer:** `StrongTypedIdAnalyzer` / `eslint-plugin-custom`
3. **What is missing:** positive test / negative test / span check
4. **Example test input:** 3–5 lines of code

**NEVER report:**
- “Analyzers need tests” without listing concrete IDs
- Issues in third-party analyzers — only custom ones
- Recommendations to add tests if the project has no custom analyzers

## Finding Schema

```text
ID
Severity: BLOCKER | CRITICAL | MAJOR | MINOR
Confidence: CONFIRMED | NEEDS_REVIEW
Category / Control
Evidence: file:line, command output, trace or reproduction
Impact
Recommended action
Owner / disposition
```

## Severity and Confidence

Severity describes impact and urgency:

| Severity | Meaning |
|----------|---------|
| **BLOCKER** | Change/release must not proceed; immediate action required |
| **CRITICAL** | High impact; fix in the current iteration |
| **MAJOR** | Degradation or defect; schedule the fix |
| **MINOR** | Improvement; backlog |

Skill-specific calibration:
- **BLOCKER** — custom analyzer without tests and used in production.
- **CRITICAL** — missing negative tests (high false-positive risk).
- **MAJOR** — tests do not verify span / location.
- **MINOR** — missing edge-case test.

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Proven by evidence: file:line, reproduction, command output |
| **NEEDS_REVIEW** | Investigation signal; requires human judgment before action |

Skill-specific calibration:
- **CONFIRMED** — the analyzer exists and there are no tests for it (fact from the codebase).
- **NEEDS_REVIEW** — tests exist, but their quality requires manual verification.

## Outputs and Downstream Consumer

Report format:

```markdown
## Analyzer Tests Audit — {date}

### Summary
| Diagnostic ID | Analyzer | Positive Tests | Negative Tests | Status |
|---------------|----------|----------------|----------------|--------|
| SAE001 | StrongTypedIdAnalyzer | yes | yes | OK |
| SAE002 | ... | no | yes | FAIL |

### Uncovered analyzers
- [ ] [CONFIRMED] `{DiagnosticId}` (`{Analyzer}`) — no positive / negative tests

### Weak tests
- [ ] [NEEDS_REVIEW] `{DiagnosticId}` — test does not verify exact span

### Recommendations
- Add an equivalent of `AnalyzerTests.cs` to the project
- Run analyzer tests in CI
```

Consumers:
- **Input from:** Code Review Agent, Architecture Tests, Version Audit.
- **Output to:** Programmer Agent (writing tests), Doc Hygiene Agent
  (updating analyzer inventory).

## Trigger or Schedule

Runs when analyzer sources, diagnostic IDs, or analyzer package versions change,
and periodically as a guardrail-health sweep.

## Limitations and Expected False Positives

- Only covers custom analyzers; third-party rule quality is out of scope.
- Test presence does not prove rule correctness — weak assertions are
  NEEDS_REVIEW signals for humans, not defects.
- Analyzer tests that compile fixtures dynamically may fail for environment
  reasons (missing reference assemblies) — distinguish from missing coverage.
