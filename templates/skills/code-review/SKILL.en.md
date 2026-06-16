---
name: code-review
description: |
  Pre-commit code review for .NET projects with agent-generated code.
  Trigger this skill right before `git commit`, when the user asks to review staged changes,
  or when the staged diff contains .NET backend files (*.cs, *.csproj, *.sln, *.props, *.targets).
  Reviews staged changes against architectural rules, EF Core / Dapper constraints, TUnit conventions,
  and security hygiene.
whenToUse:
  - Before committing .NET backend changes (pre-commit).
  - User says "pre-commit review", "review my staged changes", "check the diff", "code review".
  - Staged diff includes *.cs, *.csproj, *.sln, *.props, *.targets.
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

## Adaptation for Project

Before running, assess the project stack. If some checks are not applicable — mark them N/A and do not report as findings:
- **Single-project MVP without Clean Architecture** → skip layer checks (NetArchTest). If there are no separate Domain / Infrastructure / Application projects — the dependency rule does not apply.
- **Minimal API** → check `.RequireAuthorization()`, not `[Authorize]`.
- **`.Select()` projections to DTO** — EF Core does not track them, `.AsNoTracking()` is not required. Do not flag missing AsNoTracking on projections.
- **Dapper / Raw SQL (no EF Core)** → skip ALL EF-specific checks (AsNoTracking, Include, FindAsync, Change Tracker). Use the Dapper section below.
- **React + TypeScript frontend (JS/TS/CSS/HTML)** → this skill is not applicable. Use `frontend-code-review`.
- **Razor / Blazor / Vue / Svelte / other frameworks** → `frontend-code-review` does not cover them; create a separate skill or mark checks N/A.

---

# Pre-commit Code Review Agent

## Context Marker

When this skill is active, add `🔍` to your STARTER_CHARACTER stack.
Example: `🍀 🔍` = base rules + Code Review role active.
When re-reading this skill, prepend `♻️` to the skill marker.

## Trigger / When to invoke

Automatically activate this skill **right before every `git commit`** when staged changes include .NET backend files.
Explicit invocation: `/skill:code-review` or phrases:
- "pre-commit review"
- "review my staged changes"
- "check the diff"
- "code review"

Do NOT activate the skill when:
- Changes are React + TypeScript frontend-only (JS/TS/CSS/HTML) — use `frontend-code-review` instead.
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
- Focus on stack: .NET 10, TUnit, EF Core, Dapper, PostgreSQL, SQL Server, Minimal API.

## Pre-commit behavior
1. Read the staged diff: `git diff --cached --diff-filter=ACMR -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets'`.
2. If there are no staged .NET backend changes, tell the user there is nothing to review and stop.
3. Apply the checks below to every `+` block.
4. Produce findings in the required format.
5. State a verdict.
6. If the verdict is **CHANGES_REQUESTED**, advise the user to fix BLOCKER/CRITICAL/MAJOR issues and stage the fixes before committing. Do NOT run `git commit` yourself.

## Severity Levels
- **BLOCKER**: Security vulnerability, data loss risk, compilation error, test breakage
- **CRITICAL**: EF Core write-path with `AsNoTracking` OR Dapper SQL injection, missing `CancellationToken`, `async void`, race condition, hexagonal violation
- **MAJOR**: Missing test, exception swallowing, N+1 query, unhandled nullable, DTO mismatch
- **MINOR**: Naming inconsistency, missing XML doc, magic number
- **NIT**: Formatting, trailing whitespace, unused using

## .NET / C# Specific Checks
- **CancellationToken**: Every `async` public method MUST accept `CancellationToken ct = default`
- **EF Core Read-Path**: If read-path returns an **entity** (not a projection) — it MUST have `.AsNoTracking()`. Projections `.Select()` to DTO/record do NOT require `.AsNoTracking()` — EF Core does not track them.
- **EF Core Write-Path** (N/A for Dapper): No `.AsNoTracking()` on write operations. Exception: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`) and bulk update API (`ExecuteUpdateAsync`) — Change Tracker does not track them anyway.
- **EF Core `.Include()`**: Allowed in QueryService when justified; flag long chains without projection that cause over-fetching.
- **EF Core `.FindAsync()`**: Reasonable for read-by-primary-key; flag usage for lists/filters.
- **Dapper / Raw SQL** (N/A for EF-only):
  - **SQL Injection**: Any string interpolation (`$"..."`) or concatenation (`+`) in SQL query — BLOCKER.
  - **Parameterization**: All SQL queries MUST use parameters (`@param`). No `string.Format` in SQL.
  - **CommandTimeout**: Every `QueryAsync` / `ExecuteAsync` call MUST have `commandTimeout` or use a global default.
  - **Transactions**: Write operations (`Execute`, `ExecuteAsync`) MUST be inside `IDbTransaction`.
  - **Dynamic IN**: Dynamic `IN` lists MUST use TVP or a temp table. `string.Join` in SQL — BLOCKER.
- **Architecture**: Domain MUST NOT reference Infrastructure **only if the project has layer separation** (2+ projects with Domain / Infrastructure / Application suffixes). For single-project MVP — mark N/A.
- **DTOs**: API returns records/DTOs, not Entities directly
- **TUnit**: `[Test]` + `await Assert.That(value).IsEqualTo(expected)`. No xUnit/NUnit syntax
- **DateTime**: All backend dates MUST be UTC (`DateTime.UtcNow`, `DateTimeKind.Utc`)
- **PostgreSQL**: Column names `snake_case` via `.HasColumnName()`
- **Exception Handling**: No empty `catch { }`. At minimum log + rethrow or throw custom exception
- **Nullability**: Respect nullable reference types. `string?` vs `string` — flag mismatches
- **BUG Pattern**: Every bug fix MUST have `BUG###_DescriptiveName` test
- **Duplication (Literal)**: If validation/calculation/status check is added, verify it does not already exist in another service. Business rules belong in Domain, not copy-pasted into Application/API.
  - Automated guard: `DuplicationGuardTest.cs` catches literal copy-paste via regex.
  - Human guard: CHECKLIST.md "Semantic Duplication" catches `IsConfirmed()` vs `Status == Confirmed` — same rule, different code.

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
- **REVIEW** — possible false positive, requires human judgment before action. Use for architectural checks in projects without Clean Architecture, for EF rules with projections, etc.

## Verdict
- **APPROVED**: 0 BLOCKER/CRITICAL/MAJOR
- **APPROVED_WITH_NITS**: Only MINOR/NIT findings
- **CHANGES_REQUESTED**: Any BLOCKER/CRITICAL/MAJOR

## Execution
1. Read the staged diff (`git diff --cached --diff-filter=ACMR -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets'`).
2. For each `+` block, apply stack-specific checks.
3. Verify evidence for every finding.
4. Output findings in format above.
5. State verdict with rationale and clear next step for the user.

## Integration
- **Default trigger:** Before `git commit` (pre-commit).
- **Input from:** Staged diff in the local working tree.
- **Output to:** User (list of findings + verdict + whether it is safe to commit).
- **Also usable in PR flow:** Run after Task Compliance Agent confirms no scope creep; in that case use `git diff main...[branch]`.
