# Self-Checking Tests — Behavior Checks hardening plan

> Follow-up of the completed methodology revision 2026-07-14 (METH-001…METH-024;
> the plan document was removed after full execution — see `CHANGELOG.md` and git history).
> **Status:** partially done (SV-001…SV-003 done; SV-004, SV-005 — pending).
> **Terminology:** the workstream uses **Self-Checking Test** (xUnit Test
> Patterns, Meszaros) for the property level; **non-validating tests** remains
> the name of the trap (see [`docs/traps/non-validating-tests.md`](traps/non-validating-tests.md)).
> **Practice source:** a production .NET project under SAE audit (2026-07-14).
> The source revision is not yet fixed — **do not copy code, fixtures, or
> configs from it**; this repository keeps only principles, rules, and templates.

## Why

A discovered, executed, green test run does not prove the test can detect a
regression. The audited project contained real classes of false safety:

- test method without assertions;
- `IsNotNull()` as the only behavior check;
- assertions that can be bypassed on the successful path (inside an `if` that
  may not execute);
- tautological assertions (`Assert.That(x).IsEqualTo(x)`);
- negative-only fixtures without a positive control;
- Playwright `expect(true)`, body-only checks, `waitForTimeout`;
- test name promises a postcondition that no assertion verifies.

This workstream treats **self-checking** as: a test not only runs and passes,
but is **guaranteed to fail** when the behavior promised by its name/scenario
is broken.

## Goal

Make self-checking a verifiable property of Behavior Checks:
a constitution rule, a trap document, audit checklist items, a pattern
template, and a reproducible green/trap demonstration.

## Backlog

### SV-001 — Constitution rule (Control Foundation) ✅ done

- Added to `rules/AGENTS_TEMPLATE.md`, Tests section:
  - every test is self-checking: it fails when the behavior from its name breaks;
  - forbidden: zero-assert, `IsNotNull()`-only, assertions bypassed on the
    successful path, tautological, negative-only without positive control —
    unless the weaker check **is** the contract and the reason is documented;
  - assertions verify observable postconditions, not the fact of execution;
  - critical behavior gets fault sensitivity (mutation testing or deliberate
    fault injection).
- **Done when:** rule in template + link to the trap document. ✅

### SV-002 — Trap document ✅ done

- `docs/traps/non-validating-tests.md`: catalog of non-validating test forms,
  why they stay green, how to catch them.
- **Done when:** document in the knowledge map, cross-link from
  `false-safety.md`. ✅

### SV-003 — Audit checklists (Periodic Assurance) ✅ done

- `templates/skills/test-audit/`: items for zero-assert / `IsNotNull()`-only /
  bypassed assertions / tautology / negative-only ("Test Validity" section).
- `templates/skills/mutation-audit/`: cross-link between mutation score and
  fault sensitivity of critical tests.
- **Done when:** items present, schema-lint passes. ✅

### SV-004 — Pattern + demonstration (Behavior Checks) ⏳ pending

- `tests/patterns/` — fault-injection check template (a test that breaks
  production code locally and proves suite sensitivity).
- Green: `examples/DemoProject/`; Red: `examples/DemoProject.Traps/`
  (a non-validating test that the guardrail catches).
- **Done when:** CI green on DemoProject, Traps fails by design.

### SV-005 — Frontend (optional) ⏳ pending

- `templates/skills/frontend-code-review/`: items on `expect(true)`,
  body-only checks, `waitForTimeout` as a replacement for condition waits.
- **Done when:** items present, schema-lint passes.

## Non-goals

- No rewriting of existing DemoProject tests for mutation coverage.
- No copying of code, fixtures, or configs from the audited project — principles only.
- No mandatory mutation testing on every PR (cost — release-only or risk-trigger).

## Related artifacts

- [`docs/traps/false-safety.md`](traps/false-safety.md) — green CI ≠ working code
- [`docs/traps/non-validating-tests.md`](traps/non-validating-tests.md) — the trap itself
- [`templates/skills/test-audit/`](../templates/skills/test-audit/)
- [`templates/skills/mutation-audit/`](../templates/skills/mutation-audit/)
- [`tests/conventions/`](../tests/conventions/)
