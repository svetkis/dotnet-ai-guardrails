# AGENTS.md — Guardrails for .NET Agentic Engineering

> ⚠️ **ШАБЛОН** — Адаптируйте под свой проект перед использованием.
> Замените все блоки `[ADAPT]` на правила вашего стека.
> Не копируйте as-is: конкретные технологии ниже — пример из DemoProject.

## Semantic Anchors

This file uses established terms instead of descriptions. Each term activates a specific methodology:

| Term | Meaning |
|------|---------|
| **Read-path** | Query path: `.Select()` + `.AsNoTracking()` mandatory, change tracking forbidden |
| **Write-path** | Command path: full entity + change tracking required, `.AsNoTracking()` forbidden |
| **BUG###** | Regression test: one bug = one file `BUG###_DescriptiveName.cs`, all paths covered |
| **Ratchet** | Test inventory: count of public types / tests must not decrease |
| **Numbered Decision** | Intentional deviation: `PERF-###`, `DB-###`, `AUD-###` in comment, enforced by arch-test |

> Adapted from [Semantic Anchors](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/patterns/semantic-anchors.md) pattern.

## Permission to Push Back

You MUST:
- Ask questions when instructions are unclear or contradictory
- Flag instructions that do not make sense for the codebase
- Describe the structure you see before making changes
- Require a plan or outline before implementation if the task is complex

You MUST NOT silently comply with instructions that violate:
- EF read/write path rules below (if project uses EF Core)
- The test framework conventions defined below
- The `.Select()` requirement in read-path

> Adapted from [Silent Misalignment](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/silent-misalignment.md) anti-pattern.

## Context Markers

Start every response with a marker showing active context:
- 🍀 = these ground rules are loaded
- 🔍 = acting as Code Reviewer (diff review mode)
- ✅ = acting as Committer (commit message mode)
- ♻️ = rules were just re-read (e.g., after context loss signal)

Stack markers when multiple contexts are active: `🍀 🔍` = base rules + reviewer role.

> Adapted from [Context Markers](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/patterns/context-markers.md) pattern.

## Entity Framework

> `[ADAPT]` — Skip this section if project uses Dapper, ADO.NET, or another ORM.

### Read-path (no exceptions)
- ❌ EF queries without `.Select()` — **FORBIDDEN**
- ❌ `.Include()` in read-path — **FORBIDDEN**
- ❌ `.FindAsync()` in read-path — **FORBIDDEN**
- ✅ `.Select()` + `.AsNoTracking()` — **MANDATORY**
- ✅ Nested collections in `.Select()` — extract to separate batch query

### Write-path
- ✅ Full entity loading allowed (change tracking required)
- ✅ `.FindAsync()` allowed only in write/command scenarios
- ❌ `.AsNoTracking()` in write-path — **FORBIDDEN**

## Tests

> `[ADAPT]` — Replace with your test framework and run command.

- **Framework** — `[ADAPT]`: TUnit / xUnit / NUnit / MSTest
- Run command — `[ADAPT]`: `dotnet test` / `dotnet run --project tests/...`
- Every bug fix must include a test: `BUG###_DescriptiveName`
- Failing test first → then fix → test passes

## Database Conventions

> `[ADAPT]` — Replace with your DB naming conventions.

- Columns — `[ADAPT]`: `snake_case` / `PascalCase` / `camelCase`
- Tables — `[ADAPT]`: naming and pluralization rules
- Indexes — `[ADAPT]`: naming convention (e.g., `IX_table_column`)

## API / DTO

> `[ADAPT]` — Define your API contract rules.

- ❌ Changing DTO without updating client types — **FORBIDDEN**
- ❌ Changing Response DTO without regenerating contract snapshot — **FORBIDDEN**

## Dates

- All dates in DB: **UTC**
- Backend: `DateTime.UtcNow`, `DateTime.SpecifyKind(..., DateTimeKind.Utc)`
- JSON: `"2025-02-27T10:00:00Z"` (with Z!)
- ❌ UTC→Local conversion on backend — **FORBIDDEN**

## Commits

- Conventional Commits: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `db`
- Documentation update — in the same commit as code

## Code Review by Agent

- Before commit, run a **separate agent** to review changes and show results
- When reviewing a commit with `fix:` — check that a regression test file exists (new or modified)
- Without a regression test, the fix is not considered complete

## Numbered Decisions in Code

- Intentional deviation from standard must be documented with ID: `PERF-###`, `DB-###`, `AUD-###`
- IDs must be unique — checked by architectural test
- Example: `// PERF-022: QueryFilter removed — JOIN added 3ms to every query`

## Caching

> `[ADAPT]` — Skip if project does not use caching.

- ❌ `cache.Set()` without size limit / expiration — **FORBIDDEN** (risk of OOM)
- ✅ Explicit size or expiration — **MANDATORY**
- Keys centralized — no string literals in services
- Every write-path that changes data must invalidate related caches

## Performance

> `[ADAPT]` — Define your perf validation rules.

- After an agent's perf commit — **manual audit of write-paths**
- Agent optimizes read, human verifies write is not broken
- Load test scenario must pass before deploy (if applicable)

## Guardrails: Born from Pain

> Any guardrail (test, regex scan, arch-test, linter rule) must answer: **"What specific bug does this catch?"**

- ❌ Guardrail for a problem that has never occurred — **FORBIDDEN** (dead guardrail = over-engineering)
- ✅ Guardrail created **after** a real bug was caught and fixed — **MANDATORY**
- ✅ Deleting a guardrail with zero triggers after 3 sprints — **MANDATORY**

## Hard Prohibitions

- ❌ Commit without `dotnet build` + tests
- ❌ New env var without updating deployment docs
- ❌ Hardcoded UI strings without i18n (if project uses i18n)
- ❌ Raw SQL without explanatory comment
- ❌ Global `QueryTrackingBehavior.NoTracking` — only explicit `.AsNoTracking()` in read methods
