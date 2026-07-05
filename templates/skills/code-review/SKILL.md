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

## Адаптация под проект

Перед запуском оцени стек проекта. Если какие-то проверки неприменимы — пометь их N/A и не используй как находки:
- **Single-project MVP без Clean Architecture** → пропусти проверку слоёв (NetArchTest). Если нет отдельных проектов Domain / Infrastructure / Application — правило о зависимостях между ними неприменимо.
- **Minimal API** → проверяй `.RequireAuthorization()`, а не `[Authorize]`.
- **Проекции `.Select()` в DTO** — EF Core не отслеживает их, `.AsNoTracking()` не требуется. Не ставь флаги на отсутствие AsNoTracking на проекциях.
- **Dapper / Raw SQL (нет EF Core)** → пропусти ВСЕ EF-специфичные проверки (AsNoTracking, Include, FindAsync, Change Tracker). Используй Dapper-раздел ниже.
- **Frontend на React + TypeScript (JS/TS/CSS/HTML)** → этот скилл не применим. Используй `frontend-code-review`.
- **Razor / Blazor / Vue / Svelte / другие фреймворки** → `frontend-code-review` не покрывает их; заведи отдельный скилл или пометь проверки N/A.

---

# Pre-commit Code Review Agent

## Context Marker

Когда этот скилл активен, добавь `🔍` к своему STARTER_CHARACTER.
Пример: `🍀 🔍` = базовые правила + роль Code Review активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.

## Trigger / When to invoke

Автоматически активируй этот скилл **перед каждым `git commit`**, если в staged-изменениях есть бэкендовые .NET-файлы.
Явный вызов: `/skill:code-review` или фразы:
- "pre-commit review"
- "review my staged changes"
- "check the diff"
- "code review"
- "проревьюй staged changes"

Не активируй скилл, если:
- Изменения только в React + TypeScript frontend-файлах (используй frontend-code-review).
- Изменения в Razor/Blazor/Vue/Svelte/другом фреймворке — для них нужен отдельный скилл.
- Нет staged-изменений.
- Пользователь просит ревью всего файла без diff.

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
- **MAJOR**: Missing test, exception swallowing, N+1 query, unhandled nullable, DTO mismatch, business logic duplication
- **MINOR**: Naming inconsistency, missing XML doc, magic number
- **NIT**: Formatting, trailing whitespace, unused using

## .NET / C# Specific Checks
- **CancellationToken**: Every `async` public method MUST accept `CancellationToken ct = default`
- **EF Core Read-Path** (N/A для Dapper): Если read-path возвращает **entity** (не проекцию) — должен быть `.AsNoTracking()`. Проекции `.Select()` в DTO/record не требуют `.AsNoTracking()` — EF Core не отслеживает их.
- **EF Core Write-Path** (N/A для Dapper): No `.AsNoTracking()` on write operations. Исключение: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`) и bulk update API (`ExecuteUpdateAsync`) — Change Tracker их всё равно не отслеживает.
- **Dapper / Raw SQL** (N/A для EF-only):
  - **SQL Injection**: Любая строковая интерполяция (`$"..."`) или конкатенация (`+`) в SQL-запросе — BLOCKER.
  - **Parameterization**: Все SQL-запросы должны использовать параметры (`@param`). Нет `string.Format` в SQL.
  - **CommandTimeout**: Каждый вызов `QueryAsync` / `ExecuteAsync` должен иметь `commandTimeout` или использовать глобальный default.
  - **Transactions**: Write-операции (`Execute`, `ExecuteAsync`) должны быть внутри `IDbTransaction`.
  - **Dynamic IN**: `IN` с динамическим списком — через TVP или временную таблицу. `string.Join` в SQL — BLOCKER.
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

## Cross-Layer Drift Checks

Рефакторинг, меняющий DTO, domain events или модель, часто ломает контракты
между слоями. Для изменений, затрагивающих 2+ слоя, проверь:

- [ ] **DTO / domain event contract:** изменение поля в DTO или domain event
  сопровождается обновлением всех consumers (handlers, jobs, frontend,
  OpenAPI snapshot).
- [ ] **AuthZ / ownership drift:** изменение проверки прав в API не оставляет
  обходной путь в domain service или background job.
- [ ] **Cache invalidation drift:** добавление/изменение write-операции сопровождается
  инвалидацией кэша на всех уровнях, где этот объект кэшируется.
- [ ] **Timezone / date contract:** изменение формата хранения или передачи дат
  согласовано между UI, API, БД и job'ами.
- [ ] **Migration / runtime drift:** изменение domain model сопровождается
  миграцией; breaking change не вливается без обратной совместимости.
- [ ] **Broken invariant after refactor:** бизнес-правило, которое раньше
  проверялось в одном месте, не было случайно удалено или размазано по
  нескольким сервисам с противоречивой семантикой.
- [ ] **Что пойдёт тихо не так?** Для каждой находки задай вопрос: какой
  end-to-end инвариант сломается после мержа, хотя unit-тесты на каждый слой
  отдельно будут зелёными?

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
