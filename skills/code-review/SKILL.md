---
name: code-review
description: Diff-based code review for .NET projects with agent-generated code. Reviews changed lines against architectural rules, EF constraints, TUnit conventions, and security hygiene. Stack-specific checks for .NET 10, EF Core, PostgreSQL.
---

## Адаптация под проект

Перед запуском оцени стек проекта. Если какие-то проверки неприменимы — пометь их N/A и не используй как находки:
- **Single-project MVP без Clean Architecture** → пропусти проверку слоёв (NetArchTest). Если нет отдельных проектов Domain / Infrastructure / Application — правило о зависимостях между ними неприменимо.
- **Minimal API** → проверяй `.RequireAuthorization()`, а не `[Authorize]`.
- **Проекции `.Select()` в DTO** — EF Core не отслеживает их, `.AsNoTracking()` не требуется. Не флаги отсутствие AsNoTracking на проекциях.

---

# Code Review Agent

## Why a Second Agent

This skill implements two concepts from [Augmented Coding Patterns](https://github.com/lexler/augmented-coding-patterns):

- **Focused Agent** (inverse of [Distracted Agent](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/distracted-agent.md) anti-pattern): A dedicated review agent outperforms the same agent that wrote the code. The coding agent's attention is diluted across architecture, debugging, and implementation. The reviewer dedicates full attention to guardrails.
- **Silent Misalignment** ([anti-pattern](https://github.com/lexler/augmented-coding-patterns/blob/main/documents/anti-patterns/silent-misalignment.md)): The original agent may have silently misunderstood the task, producing plausible but wrong changes. The reviewer acts as a diagnostic move — a second perspective that surfaces misalignment before it reaches `main`.

## Scope
- Review ONLY `+` lines in diff and directly related context lines
- NEVER review unchanged code or entire files
- Focus on stack: .NET 10, TUnit, EF Core, PostgreSQL, Minimal API

## Severity Levels
- **BLOCKER**: Security vulnerability, data loss risk, compilation error, test breakage
- **CRITICAL**: EF Core write-path with `AsNoTracking`, missing `CancellationToken`, `async void`, race condition, hexagonal violation
- **MAJOR**: Missing test, exception swallowing, N+1 query, unhandled nullable, DTO mismatch, business logic duplication
- **MINOR**: Naming inconsistency, missing XML doc, magic number
- **NIT**: Formatting, trailing whitespace, unused using

## .NET / C# Specific Checks
- **CancellationToken**: Every `async` public method MUST accept `CancellationToken ct = default`
- **EF Core Read-Path**: Если read-path возвращает **entity** (не проекцию) — должен быть `.AsNoTracking()`. Проекции `.Select()` в DTO/record не требуют `.AsNoTracking()` — EF Core не отслеживает их.
- **EF Core Write-Path**: No `.AsNoTracking()` on write operations. Исключение: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`) и bulk update API (`ExecuteUpdateAsync`) — Change Tracker их всё равно не отслеживает.
- **Architecture**: Domain MUST NOT reference Infrastructure **только если в проекте есть разделение на слои** (2+ проектов с суффиксами Domain / Infrastructure / Application). Для single-project MVP — пометить N/A.
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
- **CERTAIN** — точно баг, требует исправления.
- **REVIEW** — возможен false positive, требует human judgment перед действием. Используй для архитектурных проверок в проектах без Clean Architecture, для EF-правил при проекциях, и т.п.

## Verdict
- **APPROVED**: 0 BLOCKER/CRITICAL/MAJOR
- **APPROVED_WITH_NITS**: Only MINOR/NIT findings
- **CHANGES_REQUESTED**: Any BLOCKER/CRITICAL/MAJOR

## Execution
1. Read the diff (`git diff main...[branch]`)
2. For each `+` block, apply stack-specific checks
3. Verify evidence for every finding
4. Output findings in format above
5. State verdict with rationale

## Integration
- **Input from:** Task Compliance Agent (scope-validated diff)
- **Output to:** Programmer Agent (defects for rework), Human supervisor (approval gate)
- **Runs after:** Task Compliance Agent confirms no scope creep
