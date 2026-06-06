# AGENTS.md — Guardrails for .NET Agentic Engineering

> ⚠️ **ШАБЛОН** — Адаптируйте под свой проект перед использованием.
> Замените все блоки `[ADAPT]` на правила вашего стека.
> Не копируйте as-is: конкретные технологии ниже — пример из DemoProject.
>
> **Как использовать:**
> 1. Скопируйте этот файл в корень проекта как `AGENTS.md`.
> 2. Выберите ORM-add-on:
>    - EF Core → дополните `AGENTS_TEMPLATE.efcore.md`
>    - Dapper / Raw SQL → дополните `AGENTS_TEMPLATE.dapper.md`
>    - Другой ORM / No ORM → используйте этот файл как есть, допишите свои правила

## Semantic Anchors

This file uses established terms instead of descriptions. Each term activates a specific methodology:

| Term | Meaning |
|------|---------|
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
- The test framework conventions defined below
- The hard prohibitions listed at the end of this file

> Adapted from [Silent Misalignment](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/silent-misalignment.md) anti-pattern.

## Context Markers

> Adapted from [Context Markers](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/patterns/context-markers.md) pattern.

**ALWAYS** start replies with `STARTER_CHARACTER` + space. Stack emojis when requested, don't replace.

### Base markers

| Marker | Meaning |
|--------|---------|
| 🍀 | Ground rules loaded (default `STARTER_CHARACTER`) |
| ❗️ | Flagging an error or critical warning |
| ♻️ | Rules were just re-read (e.g., after context loss signal) |
| ✨📂 | Creating new repository / project structure |

### Role markers (stackable)

| Marker | Meaning | Trigger |
|--------|---------|---------|
| ✅ | Committer role active | User says "you're a committer" |
| 🔍 | Code Reviewer role active | User asks for diff review |
| 🔴 | TDD — Red phase | Process file `tdd.md` loaded, writing failing test |
| 🌱 | TDD — Green phase | Process file `tdd.md` loaded, making test pass |
| 🌀 | TDD — Refactor phase | Process file `tdd.md` loaded, refactoring |

### Read protocol

| Situation | Marker rule |
|---|---|
| Ground rules read **for the first time** in this session | `🍀` |
| Ground rules **re-read** on explicit request or context loss | `♻️` (replaces 🍀 temporarily, then stack returns to 🍀) |
| Skill read **for the first time** | Add skill marker to stack |
| Skill **re-read** | Add `♻️` before the skill marker: `♻️ 🔍` |

### Stacking rules

- Stack markers left-to-right: `🍀 ✅` = base rules + committer role.
- Always keep a space between any emojis and the text: `🍀 ✅ Commit message`.
- Never replace a marker — add to the stack. If context ends, remove only that marker.

### Impromptu markers

When adding a crucial instruction mid-conversation, ask the agent to reply with an additional emoji:

```
> From now on treat all DateTime as UTC. Reply with 🕒 added to your markers.
```

This makes the invisible parts of context visible at a glance.

### Process file integration

In specialized process files (e.g., `tdd.md`):
```markdown
STARTER_CHARACTER = 🔴 for red test, 🌱 for green, 🌀 when refactoring, always followed by a space
```

In role definitions (e.g., committer):
```markdown
When I tell you're a committer, add ✅ to STARTER_CHARACTER emojis. Make sure there's a space between any emojis and the text.
```

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

## Code Style

- Follow **Microsoft C# Naming Conventions**: interfaces (`I` + PascalCase), types (PascalCase), members (PascalCase), parameters/locals (camelCase), private fields (`_camelCase`), static fields (`s_camelCase`), async methods (`…Async`)
- `.editorconfig` is enforced at build time (`EnforceCodeStyleInBuild = true`)
- Code-style warnings are treated as errors (`CodeAnalysisTreatWarningsAsErrors = true`)
- Run `dotnet format` before commit if IDE does not format on save

## Code Review by Agent

- Before commit, run a **separate agent** to review changes and show results
- When reviewing a commit with `fix:` — check that a regression test file exists (new or modified)
- Without a regression test, the fix is not considered complete

## Decision Guards in Code

- Intentional deviation from standard must be documented with ID: `PERF-###`, `DB-###`, `AUD-###`
- IDs must be unique — checked by architectural test
- Example: `// PERF-022: QueryFilter removed — JOIN added 3ms to every query`
- Full registry template: [`DECISION-GUARDS.md`](../skills/skeptical-ai-bootstrap/DECISION-GUARDS.md)

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
