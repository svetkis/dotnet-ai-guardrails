---
name: test-audit
description: >
  Test coverage audit. Finds gaps: services without tests, endpoints without
  integration tests, jobs without tests, dead regression tests, and uncovered
  edge cases. Runs after 3-5 features or before release.
---

# Test Audit — Skill

## Context Marker

When this skill is active, add 🧪 to your STARTER_CHARACTER stack.
Example: `🍀 🧪` = base rules + Test Audit role active.
When re-reading this skill, prepend `♻️` to the skill marker.


> Persona: QA Lead / Tech Lead. Runs after 3-5 new features or before release.
> Finds coverage gaps, dead tests, and uncovered critical paths.

## Project Adaptation

- **Single-project MVP** → skip cross-project architectural test checks.
- **No integration tests** → focus on unit + architectural tests.
- **No Jobs** → skip Background Jobs section.

## Role

You are a QA Lead in a .NET project. Your task is to find gaps in test coverage
created by agents focused on features. Do not write new tests — find gaps.

## Audit Rules

### Services
- [ ] Walk through all services in `src/*/Application/` and `src/*/Infrastructure/Services/`
- [ ] For each service — is there a test file? (not indirectly through other tests)
- [ ] Are public methods covered? (not only happy path)
- [ ] Are edge cases covered: errors, `null`, empty collections, boundary values?
- [ ] Are there concurrency tests (race conditions)?

### Endpoints
- [ ] Walk through all endpoint groups / controllers
- [ ] For each endpoint — is there an integration test (HTTP via TestServer)?
- [ ] Are statuses tested: 200, 400, 401, 403, 404, 409, 500?
- [ ] Is input validation tested (invalid DTOs)?

### Background Jobs
- [ ] Walk through all Job classes
- [ ] For each Job — is there a test? (unit or integration)
- [ ] Are covered: empty data, partial data, errors inside Job?

### Regression Tests (BUG###_)
- [ ] Does every `fix:` commit have `BUG*Tests.cs`?
- [ ] Do `BUG###_` tests actually reproduce the bug? (change code — test fails)
- [ ] Are there `BUG###_` tests that always pass (dead)?

### Characterization Tests
- [ ] Are there characterization tests for critical algorithms?
- [ ] Are they current? (behavior changed — tests updated or fail)

### Architectural Tests
- [ ] Does every rule in `AGENTS.md` have a corresponding architectural test?
- [ ] Ratchet tests: has the number of public types and tests not decreased?

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Exact file and line:** `src/.../ServiceName.cs:42`
2. **Code quote:** exact test or absence of file
3. **Rationale:** why this is a gap (reference to rule above)
4. **Fix:** specific action or code suggestion

**NEVER report:**
- "Need more tests" without specifying the service/endpoint/job
- "Coverage is low" without a specific uncovered path
- Problems you cannot confirm with code

## Severity Levels

- **BLOCKER** — critical path without tests (payments, auth, booking)
- **CRITICAL** — service/endpoint/job without tests; dead regression test
- **MAJOR** — uncovered edge case (error on empty collection)
- **MINOR** — test covers only happy path

## Confidence Level

- **CERTAIN** — service without test file; `BUG###_` test passes with broken code; endpoint without integration test
- **REVIEW** — service covered indirectly; edge case is debatable; Job covered via integration test of calling service

## Report Format

```markdown
## Test Audit — {date}

### Blocker (critical path without tests)
- [ ] [CERTAIN] `{ServiceName}` — no tests → `src/.../ServiceName.cs`
  → Fix: add `ServiceNameTests.cs` with happy path + errors

### Critical (service/endpoint/job without tests)
- [ ] [CERTAIN] `{EndpointName}` — no integration test
  → `src/.../Endpoints/EndpointName.cs`
  → Fix: add to `IntegrationTests/`

### Major (edge cases)
- [ ] [REVIEW] `{ServiceName}` — no test for empty collection
  → `src/.../ServiceName.cs:42`

### Backlog
| ID | What | Priority | Quarter |
|----|------|----------|---------|
| TD-TEST-001 | Cover `DataRetentionJob` | P2 | Q3 |
```

## Execution

Runs:
- After 3-5 new features.
- Before release.
- When CI pass rate drops due to flaky tests.

## Integration

**Input from:** Code Review Agent (observations on missed tests), Architecture Tests.
**Output to:** Programmer Agent (adding tests), Backlog Hygiene Agent (backlog items).