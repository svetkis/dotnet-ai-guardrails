# Codex (OpenAI) — Guardrails Integration

> Codex CLI от OpenAI использует `.codex/instructions.md` для project instructions.
> Минималистичный подход: один файл инструкций + CLI-запросы.

## Структура интеграции

```
.codex/
└── instructions.md                # Single instructions file

# Optional:
codex.md                           # Alternative name (in root)
```

## Конфигурация проекта

### Создать `.codex/instructions.md`

```markdown
# Project Guardrails — {ProjectName}

## You are a .NET Developer
You work in the {ProjectName} project. Follow the rules below.

## Rules
- Do not add dependencies without explicit request
- Every bug fix comes with a regression test `BUG###_DescriptiveName`
- All public async methods accept `CancellationToken ct = default`
- Do not use `async void`
- Respect nullable reference types

## Architecture
- {Clean Architecture / Vertical Slice / etc.}
- Domain does not depend on Infrastructure
- API returns DTO, not Entity

## Stack
- .NET {version}
- {EF Core / Dapper / etc.}
- {TUnit / xUnit / NUnit}
- PostgreSQL / SQL Server

## Conventions
- Interfaces: `I{Name}`
- DTOs: record (immutable)
- Jobs: `{Name}Job`
- Tests: `[Test]` + `await Assert.That(value).IsEqualTo(expected)`

## Code Review Checklist
For any change check:
- [ ] No SQL injection (parameterized queries)
- [ ] No data leak in logs/responses
- [ ] Tests for new functionality
- [ ] CancellationToken is passed through
```

## Запуск онбординга

```bash
# Install Codex CLI (if not already installed)
npm install -g @openai/codex

# Launch in the project
codex

# Inside the session:
> Scan this .NET project. Evaluate guardrails against the pyramid layers.
> Output an implementation backlog. Consider that we use {stack}.
```

## Что онбординг создаёт для код-ревью

**Цель:** после первого сканирования зафиксировать протокол ревью для Codex, привязанный к проекту.

### 1. Что решает онбординг

Онбординг должен определить:

- хватает ли review-протокола внутри `.codex/instructions.md`
- какие разделы review-протокола нужно адаптировать под стек проекта
- какие проверки надо убрать или добавить под реальный стек проекта

### 2. Что должно появиться в проекте

- обновлённый `.codex/instructions.md` с review-правилами именно этого проекта
- review-протокол встроен в тот же `.codex/instructions.md`, а не вынесен в отдельный чеклист
- отчёт с объяснением, что было адаптировано из готовых guardrails, а что пришлось описать отдельно под проект

### 3. Когда сценарий считается успешным

- review-протокол закреплён в `.codex/instructions.md`
- review-правила уже учитывают стек и архитектурные границы, а не остаются общими словами
- если стандартные проверки не подходят, это явно зафиксировано в `.codex/instructions.md`

### 4. Важная граница

Сначала онбординг собирает протокол ревью под проект. Только потом команда использует этот протокол в PR-проверках.

## Специфика Codex

### Что отличается от Kimi / Claude

| Аспект | Kimi | Claude Code | Codex |
|--------|------|-------------|-------|
| Формат правил | `.kimi/skills/*.md` | `.claude/CLAUDE.md` + commands | `.codex/instructions.md` |
| Количество файлов | Много (по файлу на скилл) | 1 + N commands | 1 файл |
| Запуск | `kimi run {name}` | `/{command}` | Прямой prompt |
| Интеграция | Ручная | Tools (bash, edit) | CLI-only |

### Нюансы Codex

1. **Один файл.** Вся конституция проекта — в одном `instructions.md`. Это значит:
   - Нельзя разбить на отдельные "аудиты" как скиллы
   - Нужно встраивать checklist'ы прямо в instructions
   - Можно использовать markdown-ссылки на внешние документы

2. **Нет custom commands.** Codex не поддерживает slash commands как Claude. Все инструкции — через prompt или `instructions.md`.

3. **Git-интеграция.** Codex умеет работать с git (видит diff, может коммитить). Это упрощает code review — агент сам может `git diff`.

4. **Прямой доступ к терминалу.** Codex может выполнять команды через `!bash` или прямо в CLI.

### Формат instructions.md

Codex использует один файл `.codex/instructions.md`. Содержание — адаптированная версия `rules/AGENTS_TEMPLATE.md`: роль, запрещённые действия, стек, архитектурные границы и review-протокол. Не копируйте шаблон целиком — оставьте только проверки, актуальные для проекта.

## Ограничения

- Нет системы скиллов — только один файл инструкций
- Нет встроенных tools (read_file, edit_file) как у Claude Code
- Работает в режиме "prompt → response", агент не всегда видит всю кодбазу
- Нет marketplace или расширяемости
- Ограничен контекстом (зависит от модели, ~128k-200k tokens)
