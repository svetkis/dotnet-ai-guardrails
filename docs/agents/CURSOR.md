# Cursor — Guardrails Integration

> Cursor IDE использует `.cursorrules` (project rules) и `.cursor/rules/` для
> контекстно-зависимых инструкций. Это VS Code-based редактор с AI-чатом
> и Composer mode.

## Структура интеграции

```
.cursor/
├── .cursorrules              # Главная конституция (аналог AGENTS.md)
└── rules/                    # Контекстно-зависимые правила (новый формат)
    ├── 001-general.md        # Общие правила для всего проекта
    ├── 002-domain.md         # Правила для Domain слоя
    ├── 003-infrastructure.md # Правила для Infrastructure
    ├── 004-api.md            # Правила для API слоя
    ├── 005-tests.md          # Правила для тестов
    └── 006-audits.md         # Prompt-шаблоны для аудитов
```

## Конфигурация проекта

### 1. Создать `.cursorrules`

Это аналог корневого `AGENTS.md` — единый файл с конституцией проекта:

```markdown
# Project Guardrails — {ProjectName}

## Миссия
{описание проекта}

## Правила для AI
- Не добавляй зависимости без явного запроса
- Не меняй структуру папок без согласования
- Каждый баг-фикс — с regression тестом (BUG###_DescriptiveName.cs)
- Все публичные async методы принимают CancellationToken
- Не используй preview-версии SDK и NuGet-пакетов
- ...

## Архитектура
- {Clean / Vertical Slice / etc.}
- Зависимости между слоями: Domain → Application → Infrastructure
- Domain не зависит от Infrastructure

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...

## Conventions
- Naming: ...
- Commits: ...
- Tests: `dotnet run --project`, не `dotnet test`
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

- Нулевые внешние зависимости (кроме стандартной библиотеки)
- Все сущности — immutable records или с private setters
- Миграция обязательна при изменении модели
- Нет прямых вызовов DB, HTTP, File IO
```

```markdown
---
description: Infrastructure layer rules
glob: src/**/Infrastructure/**/*.cs
alwaysApply: false
---

# Infrastructure Layer Rules

- Select() обязателен в read-path (запрещен .Include())
- FindAsync() только в write-path (Command handlers)
- Каждый cache.Set() — с указанием размера
- Все сервисы реализуют интерфейс из Application
```

```markdown
---
description: Test patterns and conventions
glob: tests/**/*.cs
alwaysApply: false
---

# Test Conventions

- TUnit + `dotnet run --project`
- Каждый баг — файл BUG###_DescriptiveName.cs
- Архитектурные тесты: NetArchTest
- Не использовать `dotnet test` (ловушка "0 tests ran")
```

```markdown
---
description: Audit prompts
glob: ""
alwaysApply: false
---

# Audit Prompts

## Code Review
Проведи code review текущих изменений:
1. Получи diff: git diff origin/main
2. Проверь только + строки
3. Ищи: SQL injection, missing await, XSS, data leak
4. Для каждой находки укажи файл:строка + цитату кода
5. Выдай вердикт: APPROVED / CHANGES_REQUESTED

## Security Audit
Проверь файлы на утечки данных:
- Логи: нет PII, токенов, connection strings
- API ответы: нет лишних полей
- Exception messages: нет SQL, нет путей ФС
- Все новые endpoints покрыты авторизацией
```

## Запуск онбординга

### Вариант A: Chat mode (Ctrl+L)

```
Просканируй этот .NET-проект. Оцени guardrails по 5 слоям пирамиды.
Выдай бэклог внедрения. Учитывай, что мы используем {стек}.
```

### Вариант B: Composer mode (Ctrl+I)

```
Создай архитектурные тесты для этого проекта:
1. Проверь зависимости между слоями (Domain → Application → Infrastructure)
2. Найди все сервисы и проверь что они имеют интерфейсы
3. Создай ratchet-тест для контроля публичных типов в Application-слое и количества тестов
```

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
