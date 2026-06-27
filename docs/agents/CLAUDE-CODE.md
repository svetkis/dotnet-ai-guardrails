# Claude Code — Guardrails Integration

> Claude Code (Anthropic) использует `CLAUDE.md` для project instructions
> и `.claude/commands/` для custom slash commands.
> У него другая ментальная модель: не "скиллы", а "проектные инструкции + команды".

## Структура интеграции

```
.claude/
├── CLAUDE.md                      # Main constitution (analog of AGENTS.md)
├── settings.json                  # Project settings
└── commands/                      # Custom slash commands
    ├── code-review.md             # /code-review
    ├── task-compliance.md         # /task-compliance
    ├── security-audit.md          # /security-audit
    ├── complexity-audit.md        # /complexity-audit
    ├── allocation-budget-audit.md # /allocation-budget-audit
    └── {project-specific}.md      # Custom commands
```

## Конфигурация проекта

### 1. Создать `CLAUDE.md`

Это аналог `AGENTS.md` + `CONVENTIONS.md` вместе:

```markdown
# Project Guardrails — {ProjectName}

## Mission
{project description}

## Rules for Claude
- Do not add dependencies without explicit request
- Do not change folder structure without agreement
- Every bug fix comes with a regression test
- All public async methods accept CancellationToken
- ...

## Architecture
- {Clean / Vertical Slice / etc.}
- Dependencies between layers: ...

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...

## Conventions
- Naming: ...
- Commits: ...
- Tests: ...
```

### 2. Создать `.claude/settings.json`

```json
{
  "project": {
    "instructions": "CLAUDE.md"
  },
  "permissions": {
    "allow_bash": true,
    "allow_edit": true,
    "allow_read": true
  }
}
```

### 3. Создать custom commands

Каждая команда — это Markdown-файл в `.claude/commands/`:

```markdown
# code-review

## Description
Conduct code review of current changes.

## Instructions
1. Get diff: `git diff origin/main`
2. Check only `+` lines
3. Look for: SQL injection, missing await, XSS, data leak
4. For each finding specify file:line + code quote
5. Issue verdict: APPROVED / CHANGES_REQUESTED

## Severity
- BLOCKER: Security, data loss, compilation error
- CRITICAL: async void, missing CancellationToken, race condition
- MAJOR: Missing test, exception swallowing
- MINOR: Naming, magic number
```

Запуск: `/code-review` внутри Claude Code.

## Запуск онбординга

```bash
# Launch Claude Code in the project
claude

# Inside the session:
> Scan this .NET project. Evaluate guardrails against the pyramid layers.
> Output an implementation backlog. Consider that we use {stack}.
```

Или создать custom command:

```markdown
# .claude/commands/onboarding.md

## Description
Scan the project and output a guardrails implementation backlog.

## Instructions
1. Find all `.csproj` and determine the stack
2. Evaluate layers 1.1→2.3:
   - 1.1 Compiler: TreatWarningsAsErrors? Nullable?
   - 1.2 Architecture: any arch tests?
   - 1.3 Tests: framework, coverage, "0 ran"?
   - 1.4 Code Review: any rules?
   - 1.5 Smoke: any critical scenario runs?
   - 2.1 E2E / MCP: any integration tests?
   - 2.2 Audits: have any been run?
   - 2.3 Load: any load tests?
3. For each layer: Adapt / Create / Skip
4. Output a report in markdown format
```

## Что онбординг создаёт для код-ревью

**Цель:** после оценки проекта получить не абстрактный “ежедневный цикл”, а конкретную Claude-команду для код-ревью под ваш стек.

### 1. Что решает онбординг

Онбординг должен определить:

- хватит ли стандартной команды `code-review`
- нужно ли адаптировать шаблон под стек проекта
- нужна ли отдельная команда для ревью: например `code-review-dapper` или `code-review-razor`

### 2. Что должно появиться в проекте

- `.claude/commands/code-review.md` или `.claude/commands/code-review-{context}.md`
- обновлённый `CLAUDE.md`, если review-правила надо зафиксировать и в общей конституции
- отчёт или backlog-пункт с объяснением, что адаптировали и что не подошло из готовых артефактов

### 3. Когда сценарий считается успешным

- у команды есть точная команда `/...` для ревью в этом проекте
- команда знает, почему это именно `code-review`, а не другой вариант
- если стандартный шаблон не подходит, это отражено в явной отдельной команде под проект, а не в устной адаптации

### 4. Важная граница

Онбординг сначала создаёт или адаптирует команду для ревью под проект. Только потом эта команда становится частью PR-потока.

## Специфика Claude Code

### Что отличается от Kimi

| Аспект | Kimi | Claude Code |
|--------|------|-------------|
| Формат правил | `.kimi/skills/{name}/SKILL.md` | `.claude/CLAUDE.md` + `.claude/commands/*.md` |
| Запуск скилла | `kimi run {name}` | `/{command-name}` в чате |
| Контекст | Ограничен окном | 200k tokens + tools (read, edit, bash) |
| Интеграция CI | Ручной запуск | Может запускать bash-скрипты |
| Автозапуск | Нет | Нет (но есть tools) |

### Нюансы Claude Code

1. **CLAUDE.md — единый файл.** Нельзя разбить на 10 отдельных скиллов как в `.kimi/skills/`. Но можно сделать:
   - Основной `CLAUDE.md` с конституцией
   - `.claude/commands/` с конкретными задачами (аудит, review)

2. **Tools.** Claude Code имеет встроенные tools:
   - `read_file` — читает файлы
   - `edit_file` — редактирует
   - `bash` — выполняет команды
   - Это меняет формат anti-hallucination protocol: агент реально видит файлы

3. **Context management.** Claude умеет `/add` файлы в контекст и `/compact` историю. В скиллах это неявно.

4. **Не требует установки скиллов.** Просто создаёшь файлы в `.claude/` — и они работают.

## Ограничения

- Нет встроенной системы "скиллов" как у Kimi — только project instructions + commands
- Нет marketplace скиллов
- Каждый новый проект требует ручного создания `.claude/`
- Не умеет автоматически читать `.claude/commands/` при старте (нужно явно вызывать `/command`)
