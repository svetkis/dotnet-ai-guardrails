# Non-Validating Tests — Green Without Verification

> **Status:** in force — wired into `rules/AGENTS_TEMPLATE.md` (Tests), Test Audit and Mutation Audit skills. Part of the [Self-Checking Tests workstream](../SELF-CHECKING-TESTS-WORKSTREAM.md) (SV-004/SV-005 demo patterns still pending).

A test can be discovered, executed, and green while proving nothing about the
behavior named by the test.

## Terminology

Three distinct properties (do not merge them):

| Term | Origin | Meaning |
|------|--------|---------|
| **Self-Checking Test** | industry-standard (xUnit Test Patterns, Meszaros) | The test determines pass/fail automatically, without manual interpretation of results |
| **Assertion Reachability** | SAE-specific | No successful execution path bypasses the assertions |
| **Fault Sensitivity** | borrowed (mutation testing) | The test fails when a relevant defect is present (mutation, or the original bug) |

Self-checking is the baseline this trap assumes. The trap itself lives in the
other two: assertions exist but are unreachable on the green path, or reachable
but insensitive to the defect.

## The trap

AI agents are good at producing tests that look structurally complete:

```text
Arrange → Act → Assert → green build
```

The presence of an assertion is not enough. A test is useful only if a relevant
defect makes it fail. Non-validating tests preserve the appearance of coverage
while allowing broken behavior through CI.

## Common forms

| Pattern | Why it stays green |
|---------|--------------------|
| Zero-assert test | Nothing is verified; execution without exception passes |
| `IsNotNull()` as the only check | Any non-null wrong result passes |
| Assertion inside a conditional branch | The branch may never execute |
| Tautological assertion (`x == x`, `expect(true)`) | Cannot fail by construction |
| Negative-only fixture without positive control | Proves absence of one defect, not presence of behavior |
| Mock-of-mock | The test verifies mock wiring, not system behavior |
| Test name promises more than assertions check | Readers trust the name; the gap is invisible |
| `waitForTimeout` instead of condition wait (frontend) | Timing races pass on fast machines, and body-only checks miss behavior |

## Why agents produce them

Observed mechanism (no mind-reading required):

- The task says "add tests" but does not state what must be verified. The
  formal completion criteria — file exists, test discovered, run green — are
  satisfiable without behavior checks.
- Review diffs show `Assert.` calls; whether the assertion is reachable and
  sensitive to the defect is not visible without control-flow analysis or
  fault injection, so reviews pass.

## Guardrails

1. **Constitution rule** (`rules/AGENTS_TEMPLATE.md`): tests must be
   self-checking with assertion reachability and fault sensitivity — a test
   must fail when the behavior promised by its name is broken. Zero-assert,
   `IsNotNull()`-only, bypassed, tautological, and negative-only tests are
   forbidden unless the weaker check *is* the contract and the reason is
   documented.
2. **Compile-time analyzers** (`examples/DemoProject/src/DemoProject.Analyzers/`):
   SAE006-SAE009 detect non-validating tests directly in the IDE / build:
   zero-assert, null-only, bypassed, tautological. See
   `tests/conventions/AnalyzerDiagnostics.md` for the full diagnostic catalog.
3. **Deliberate fault injection:** for critical behavior, break the production
   code locally and confirm the test fails. If it doesn't — the test is dead.
4. **Mutation testing** (risk-trigger / release): mutation score on critical
   assemblies measures fault sensitivity of the whole suite. See
   `templates/skills/mutation-audit/`.
5. **Test audit checklist** (`templates/skills/test-audit/`): scan for the
   forms above; each hit is an investigation signal, not an automatic defect.
6. **Review anchor:** in code review, check that assertions verify observable
   postconditions (state, output, effects) — not merely execution or object
   existence.

## Relation to other traps

- [false-safety](false-safety.md) — green CI ≠ working code; non-validating
  tests are the test-level instance of that trap.
- [over-engineering](over-engineering.md) — the opposite failure: test fixtures
  so complex that nobody notices they verify mocks, not behavior.
