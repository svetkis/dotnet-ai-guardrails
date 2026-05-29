# Claude Code — Guardrails Integration

> Claude Code (Anthropic) использует `CLAUDE.md` для project instructions
> и `.claude/commands/` для custom slash commands.
> У него другая ментальная модель: не "скиллы", а "проектные инструкции + команды".

## Структура интеграции

```
.claude/
├── CLAUDE.md                      # Главная конституция (аналог AGENTS.md)
├── settings.json                  # Настройки проекта
└── commands/                      # Custom slash commands
    ├── code-review.md             # /code-review
    ├── task-compliance.md         # /task-compliance
    ├── security-audit.md          # /security-audit
    └── {project-specific}.md      # Кастомные команды
```

## Конфигурация проекта

### 1. Создать `CLAUDE.md`

Это аналог `AGENTS.md` + `CONVENTIONS.md` вместе:

```markdown
# Project Guardrails — {ProjectName}

## Миссия
{описание проекта}

## Правила для Claude
- Не добавляй зависимости без явного запроса
- Не меняй структуру папок без согласования
- Каждый баг-фикс — с regression тестом
- Все публичные async методы принимают CancellationToken
- ...

## Архитектура
- {Clean / Vertical Slice / etc.}
- Зависимости между слоями: ...

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

## Описание
Провести code review текущих изменений.

## Инструкции
1. Получи diff: `git diff origin/main`
2. Проверь только `+` строки
3. Ищи: SQL injection, missing await, XSS, data leak
4. Для каждой находки укажи файл:строку + цитату кода
5. Выдай вердикт: APPROVED / CHANGES_REQUESTED

## Severity
- BLOCKER: Security, data loss, compilation error
- CRITICAL: async void, missing CancellationToken, race condition
- MAJOR: Missing test, exception swallowing
- MINOR: Naming, magic number
```

Запуск: `/code-review` внутри Claude Code.

## Запуск онбординга

```bash
# Запустить Claude Code в проекте
claude

# Внутри сессии:
> Просканируй этот .NET-проект. Оцени guardrails по 5 слоям пирамиды.
> Выдай бэклог внедрения. Учитывай, что мы используем {стек}.
```

Или создать custom command:

```markdown
# .claude/commands/onboarding.md

## Описание
Просканировать проект и выдать бэклог внедрения guardrails.

## Инструкции
1. Найди все `.csproj` и определи стек
2. Оцени 5 слоёв пирамиды:
   - Компилятор: TreatWarningsAsErrors? Nullable?
   - Архитектура: есть ли арх. тесты?
   - Тесты: фреймворк, coverage, "0 ran"?
   - Code Review: есть ли правила?
   - E2E: есть ли интеграционные тесты?
3. Для каждого слоя: Адаптировать / Создать / Пропустить
4. Выдай отчёт в формате markdown
```

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
