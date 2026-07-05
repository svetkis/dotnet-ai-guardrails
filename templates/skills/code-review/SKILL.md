---
name: code-review
description: |
  Pre-commit code review for backend projects with agent-generated code.
  Trigger this skill right before `git commit`, when the user asks to review staged changes,
  or when the staged diff contains backend source files.
  Reviews staged changes against architectural rules, ORM constraints, test conventions,
  and security hygiene.
whenToUse:
  - Before committing backend changes (pre-commit).
  - User says "pre-commit review", "review my staged changes", "check the diff", "code review".
  - Staged diff includes source files of the project's backend language.
triggers:
  - pre-commit
  - review staged diff
  - code review
  - check diff before commit
invocation:
  manual: true
  auto: true
version: 1.0.0
---

## Portable core

- Review **only staged changes** (`git diff --cached`) and only added (`+`) lines with immediate context.
- Never review unchanged code or entire files without a diff.
- Every finding must include: exact path and line, code quote, violated rule, concrete fix, and self-correction evidence.
- Verdict: `APPROVED` / `APPROVED_WITH_NITS` / `CHANGES_REQUESTED`.

## Requires adaptation

- Language / stack: C#, Java, Python, Go, TypeScript, etc.
- Authorization framework: MVC attributes, Minimal API middleware, decorators, guards.
- ORM / data access: EF Core, Dapper, Hibernate, SQLAlchemy, raw SQL, etc.
- Architectural boundaries: Clean Architecture, Vertical Slices, Ports & Adapters, single-project MVP.
- Test framework and bug-regression test naming convention.

## Not applicable when

- Changes are only in frontend / mobile / desktop UI covered by a separate skill.
- There are no staged changes.
- The user asks for a full-file review without a diff.

---

## Adaptation for Project

Before running, assess the project stack. If some checks are not applicable — mark them N/A and do not report as findings:
- **Single-project MVP without layered architecture** → skip cross-project dependency checks. If there are no separate Domain / Infrastructure / Application projects — the dependency rule does not apply.
- **Minimal API / non-MVC** → check middleware / fluent authorization, not MVC attributes.
- **ORM with projection DTOs** — if the ORM does not track projections, `AsNoTracking` (or equivalent) is not required. Do not flag missing read-only mode on projections.
- **Dapper / raw SQL (no ORM tracking)** → skip ORM-specific checks (change tracking, eager loading, async find helpers). Use the raw SQL section.
- **React + TypeScript frontend** → this skill is not applicable. Use `frontend-code-review`.
- **Razor / Blazor / Vue / Svelte / other frameworks** → `frontend-code-review` does not cover them; create a separate skill or mark checks N/A.

---

# Pre-commit Code Review Agent

## Context Marker

When this skill is active, add `🔍` to your `STARTER_CHARACTER` stack.
Example: `🍀 🔍` = base rules + Code Review role active.
When re-reading this skill, prepend `♻️` to the skill marker.

## Trigger / When to invoke

Automatically activate this skill **right before every `git commit`** when staged changes include backend source files.
Explicit invocation: `/skill:code-review` or phrases:
- "pre-commit review"
- "review my staged changes"
- "check the diff"
- "code review"

Do NOT activate the skill when:
- Changes are frontend-only (use `frontend-code-review` instead).
- Changes are Razor/Blazor/Vue/Svelte/other framework — they need a separate skill.
- There are no staged changes.
- The user asks for a full-file review without a diff.

## Why a Second Agent

This skill implements two concepts from [Augmented Coding Patterns](https://github.com/lexler/augmented-coding-patterns):

- **Focused Agent** (inverse of [Distracted Agent](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/distracted-agent.md) anti-pattern): A dedicated review agent outperforms the same agent that wrote the code. The coding agent's attention is diluted across architecture, debugging, and implementation. The reviewer dedicates full attention to guardrails.
- **Silent Misalignment** ([anti-pattern](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/silent-misalignment.md)): The original agent may have silently misunderstood the task, producing plausible but wrong changes. The reviewer acts as a diagnostic move — a second perspective that surfaces misalignment before it reaches `main`.

## Scope
- Review ONLY staged changes (`git diff --cached`).
- Review ONLY `+` lines in the staged diff and directly related context lines.
- NEVER review unchanged code or entire files.
- Focus on the project's backend stack.

## Pre-commit behavior
1. Read the staged diff: backend source files only.
2. If there are no staged backend changes, tell the user there is nothing to review and stop.
3. Apply the checks below to every `+` block.
4. Produce findings in the required format.
5. State a verdict.
6. If the verdict is **CHANGES_REQUESTED**, advise the user to fix BLOCKER/CRITICAL/MAJOR issues and stage the fixes before committing. Do NOT run `git commit` yourself.

## Severity Levels
- **BLOCKER**: Security vulnerability, data loss risk, compilation error, test breakage
- **CRITICAL**: ORM write-path with read-only tracking OR SQL injection, missing `CancellationToken` (if project uses async), `async void`, race condition, architecture violation
- **MAJOR**: Missing test, exception swallowing, N+1 query, unhandled nullable, DTO mismatch, business logic duplication
- **MINOR**: Naming inconsistency, missing XML doc, magic number
- **NIT**: Formatting, trailing whitespace, unused import/using

## Backend-specific checks (adapt to your stack)

### If project uses async public methods
- Every public `async` method accepts `CancellationToken ct = default`.

### If project uses ORM with change tracking
- **Read-path:** if a method returns an entity (not a projection), it must have explicit read-only mode (`AsNoTracking` or equivalent). Projections to DTO/record do not require read-only mode if the ORM does not track them.
- **Write-path:** no read-only mode on write operations. Exception: raw SQL and bulk API that the change tracker does not track anyway.

### If project uses raw SQL / Dapper / ADO.NET
- **SQL Injection:** any string interpolation or concatenation in SQL query — BLOCKER.
- **Parameterization:** all SQL queries use parameters. No `string.Format` in SQL.
- **CommandTimeout:** every query/execute call has a timeout or uses a global default.
- **Transactions:** write operations must be inside a transaction.
- **Dynamic IN:** dynamic `IN` lists must use TVP, temp table, or ORM equivalent. `string.Join` in SQL — BLOCKER.

### If project uses layered / hexagonal architecture
- Domain MUST NOT reference Infrastructure **only if the project has layer separation** (2+ projects with Domain / Infrastructure / Application suffixes). For single-project MVP — mark N/A.

### API / DTO
- API returns DTOs/records, not entities directly.
- Naming is consistent: `OrderResponse`, `CreateOrderRequest`, `OrderListItem` — not generic `OrderDto` everywhere.

### Tests
- The project uses a single test framework (no mixing of xUnit/NUnit/MSTest/Jest/Mocha without reason).
- Every bug fix has a named regression test per project convention.

### Common hygiene
- Dates: if the project uses UTC/ISO 8601 — check compliance.
- Exception handling: no empty `catch { }`. At minimum log + rethrow or throw custom exception.
- Nullability: respect nullable reference types / optional types.
- **Duplication (Literal):** if validation/calculation/status check is added, verify it does not already exist in another service. Business rules belong in Domain, not copy-pasted into Application/API.
  - Automated guardrail: an equivalent `DuplicationGuardTest` catches literal copy-paste.
  - Human guard: CHECKLIST.md "Semantic Duplication" catches `IsConfirmed()` vs `Status == Confirmed` — same rule, different code.

## Cross-Layer Drift Checks

Refactors that change DTOs, domain events, or models often break contracts
between layers. For changes touching 2+ layers, check:

- [ ] **DTO / domain event contract:** a field change in a DTO or domain event
  is accompanied by updates to all consumers (handlers, jobs, frontend,
  API schema snapshot).
- [ ] **AuthZ / ownership drift:** a permission check change in API does not leave
  a bypass in domain service or background job.
- [ ] **Cache invalidation drift:** a write operation addition/change is accompanied by
  cache invalidation on all levels where this object is cached.
- [ ] **Timezone / date contract:** a storage or transmission format change
  is agreed between UI, API, DB, and jobs.
- [ ] **Migration / runtime drift:** a domain model change is accompanied by
  a migration; breaking changes are not merged without backward compatibility.
- [ ] **Broken invariant after refactor:** a business rule that was previously
  checked in one place has not been accidentally removed or smeared across
  several services with contradictory semantics.
- [ ] **What could silently go wrong?** For every finding ask: which
  end-to-end invariant will break after merge, even though per-layer unit tests
  are green?

## Project-specific examples

> The examples below illustrate applying portable checks to a concrete stack. Replace with your own stack.

### Example: .NET 10 + EF Core + PostgreSQL + Minimal API

- **EF Core Read-Path:** if read-path returns an entity (not a projection) — it must have `.AsNoTracking()`. Projections `.Select()` to DTO/record do NOT require `.AsNoTracking()`.
- **EF Core Write-Path:** no `.AsNoTracking()` on write operations. Exception: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).
- **Dapper / Raw SQL:** interpolation in SQL — BLOCKER; parameterization required; `CommandTimeout`; `IDbTransaction` for writes; dynamic `IN` via TVP/temp table.
- **Architecture:** Domain MUST NOT reference Infrastructure **only if the project has layer separation**.
- **Tests:** `[Test]` + `await Assert.That(value).IsEqualTo(expected)` (TUnit convention). No xUnit/NUnit syntax.
- **DateTime:** All backend dates MUST be UTC (`DateTime.UtcNow`, `DateTimeKind.Utc`).
- **PostgreSQL:** Column names `snake_case` via `.HasColumnName()`.
- **BUG Pattern:** Every bug fix MUST have `BUG###_DescriptiveName` test.

## ANTI-HALLUCINATION Protocol
Every finding MUST include:
1. **Exact file path** and **line number**
2. **Quoted snippet** (3-5 lines)
3. **Rule violated** (from checks above)
4. **Fix**: specific action or code suggestion
5. **Self-Correction**: "Can I point to exact line? Did I read the file myself?"

If you cannot satisfy 1-4, you MUST NOT report the finding.

## Output Format
```
[SEVERITY] [CONFIDENCE] Title | File:line | Rule | Evidence: "quoted snippet" | Fix: action
```

**Confidence Level:**
- **CERTAIN** — definitely a bug, requires fixing.
- **REVIEW** — possible false positive, requires human judgment before action. Use for architectural checks in projects without Clean Architecture, for ORM rules with projections, etc.

## Verdict
- **APPROVED**: 0 BLOCKER/CRITICAL/MAJOR
- **APPROVED_WITH_NITS**: Only MINOR/NIT findings
- **CHANGES_REQUESTED**: Any BLOCKER/CRITICAL/MAJOR

## Execution
1. Read the staged diff (backend source files only).
2. For each `+` block, apply stack-specific checks.
3. Verify evidence for every finding.
4. Output findings in format above.
5. State verdict with rationale and clear next step for the user.

## Integration
- **Default trigger:** Before `git commit` (pre-commit).
- **Input from:** Staged diff in the local working tree.
- **Output to:** User (list of findings + verdict + whether it is safe to commit).
- **Also usable in PR flow:** Run after Task Compliance Agent confirms no scope creep; in that case use `git diff main...[branch]`.
