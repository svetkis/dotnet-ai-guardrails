# Cursor — Guardrails Integration

> Cursor IDE использует `.cursorrules` (project rules) и `.cursor/rules/` для
> контекстно-зависимых инструкций. Это VS Code-based редактор с AI-чатом
> и Composer mode.

## Структура интеграции

```
.cursor/
├── .cursorrules              # Main constitution (analog of AGENTS.md)
└── rules/                    # Context-dependent rules (new format)
    ├── 001-general.md        # General rules for the entire project
    ├── 002-domain.md         # Rules for Domain layer
    ├── 003-infrastructure.md # Rules for Infrastructure
    ├── 004-api.md            # Rules for API layer
    ├── 005-tests.md          # Rules for tests
    └── 006-audits.md         # Prompt templates for audits
```

## Конфигурация проекта

### 1. Создать `.cursorrules`

Это аналог корневого `AGENTS.md` — единый файл с конституцией проекта:

```markdown
# Project Guardrails — {ProjectName}

## Mission
{project description}

## Rules for AI
- Do not add dependencies without explicit request
- Do not change folder structure without agreement
- Every bug fix comes with a regression test (BUG###_DescriptiveName.cs)
- All public async methods accept CancellationToken
- Do not use preview versions of SDK and NuGet packages
- ...

## Architecture
- {Clean / Vertical Slice / etc.}
- Dependencies between layers: Domain → Application → Infrastructure
- Domain does not depend on Infrastructure

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...

## Conventions
- Naming: ...
- Commits: ...
- Tests: `dotnet run --project`, not `dotnet test`
```

### 2. Создать `.cursor/rules/` (рекомендуется для проектов 50k+ LOC)

Разбивать `.cursorrules` на контекстно-зависимые файлы:

```markdown
---
description: Domain layer rules
glob: src/**/Domain/**/*.cs
alwaysApply: false
---

# Domain Layer Rules

- Zero external dependencies (except standard library)
- All entities are immutable records or have private setters
- Migration is mandatory when changing the model
- No direct DB, HTTP, File IO calls
```

```markdown
---
description: Infrastructure layer rules
glob: src/**/Infrastructure/**/*.cs
alwaysApply: false
---

# Infrastructure Layer Rules

- Select() is mandatory in read-path (.Include() forbidden)
- FindAsync() only in write-path (Command handlers)
- Every cache.Set() — with size specified
- All services implement an interface from Application
```

```markdown
---
description: Test patterns and conventions
glob: tests/**/*.cs
alwaysApply: false
---

# Test Conventions

- TUnit + `dotnet run --project`
- Every bug is a file BUG###_DescriptiveName.cs
- Architecture tests: NetArchTest
- Do not use `dotnet test` ("0 tests ran" trap)
```

```markdown
---
description: Audit prompts
glob: ""
alwaysApply: false
---

# Audit Prompts

## Code Review
Conduct code review of current changes:
1. Get diff: git diff origin/main
2. Check only + lines
3. Look for: SQL injection, missing await, XSS, data leak
4. For each finding specify file:line + code quote
5. Issue verdict: APPROVED / CHANGES_REQUESTED

## Security Audit
Check files for data leaks:
- Logs: no PII, tokens, connection strings
- API responses: no extra fields
- Exception messages: no SQL, no FS paths
- All new endpoints are covered by authorization
```

## Запуск онбординга

### Вариант A: Chat mode (Ctrl+L)

```
Scan this .NET project. Evaluate guardrails against the pyramid layers.
Output an implementation backlog. Consider that we use {stack}.
```

### Вариант B: Composer mode (Ctrl+I)

```
Scan this .NET project as a multi-step onboarding:
1. Find all `.csproj` and determine the stack
2. Assess guardrails for layers 1.1→2.3
3. Output an implementation backlog with Adapt / Create / Skip decisions
```

## Что онбординг создаёт для код-ревью

**Цель:** после первичной оценки получить артефакт для код-ревью именно этого проекта, а не выдуманный универсальный цикл.

### 1. Что решает онбординг

Онбординг должен определить:

- достаточно ли общих review-правил в `.cursorrules`
- нужно ли вынести review-проверки в `.cursor/rules/` по слоям или контекстам
- нужен ли отдельный запрос или notepad для ревью под проект

### 2. Что должно появиться в проекте

- review-правила в `.cursorrules` и/или `.cursor/rules/`
- при необходимости отдельный review prompt / notepad под ваш стек
- явная фиксация, какие проверки вычеркнуты как N/A и что добавлено специально под проект

### 3. Когда сценарий считается успешным

- Cursor получает review-контекст из проектных файлов, а не из случайного чата
- команда понимает, где лежит основной запрос для ревью в этом проекте
- если базовые правила не подходят, это отражено в правилах именно этого проекта, а не “додумывается” каждым разработчиком отдельно

### 4. Важная граница

Онбординг сначала фиксирует правила и запросы для ревью в проекте. Уже потом команда использует их на PR и задачах по рефакторингу.

## Специфика Cursor

### Что отличается от Kimi / Claude Code

| Аспект | Kimi | Claude Code | Cursor |
|--------|------|-------------|--------|
| Формат правил | `.kimi/skills/{name}/SKILL.md` | `.claude/CLAUDE.md` + commands | `.cursorrules` + `.cursor/rules/*.md` |
| Контекст правил | Ручной выбор скилла | Project-wide + commands | Автоматический по glob-маске |
| Запуск | `kimi run {name}` | `/{command}` в чате | Chat / Composer / Tab |
| IDE | CLI | CLI | VS Code-based GUI |
| Tools | Ограниченно | Bash, edit, read | Inline edits, chat, composer |
| Context | ~200k tokens | ~200k tokens | ~200k tokens |

### Нюансы Cursor

1. **Context-aware rules.** Cursor автоматически подключает правила из `.cursor/rules/` на основе `glob` файла, над которым работаешь. Это ближе всего к иерархическим `AGENTS.md` — правила Domain слоя подключаются только когда редактируешь Domain.

2. **`.cursorrules` vs `.cursor/rules/`.**
   - `.cursorrules` — legacy формат, один файл на проект
   - `.cursor/rules/` — новый формат, поддерживает multiple files с YAML frontmatter
   - Рекомендуется использовать `.cursor/rules/` для больших проектов

3. **Inline edits.** Cursor умеет редактировать код прямо в редакторе (Tab completion, Cmd+K). Это меняет формат anti-hallucination protocol: агент видит контекст открытого файла.

4. **Composer mode.** Многошаговые задачи (создание фичи end-to-end) лучше делать в Composer, а не в Chat. Composer помнит контекст между шагами.

5. **Notepads.** Cursor поддерживает Notepads — markdown-файлы с контекстом, которые можно прикреплять к чату. Это аналог скиллов Kimi.

## Ограничения

- Нет встроенной системы "скиллов" как у Kimi — только project rules + notepads
- Нет marketplace скиллов
- Нет CLI-интерфейса для автоматического запуска (в отличие от `kimi run` или `claude`)
- Rules не автозапускаются — нужно явно работать с файлом, чтобы правило подключилось
- Контекст ограничен окном модели (~200k tokens)
- Не умеет выполнять bash-команды в отличие от Claude Code
