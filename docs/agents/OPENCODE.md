# OpenCode — Guardrails Integration

> OpenCode — open-source агент для разработки (опенсорс-альтернатива проприетарным инструментам).
> Может работать как VS Code extension или standalone.
> Формат конфигурации зависит от конкретной реализации.

## Структура интеграции

OpenCode обычно использует один из форматов:

### Вариант A: Markdown instructions (похоже на Codex)

```
.opencode/
└── instructions.md                # Project instructions
```

### Вариант B: JSON/YAML config

```
.opencode/
├── config.json                    # Settings
├── prompts/
│   ├── code-review.md
│   ├── architecture-audit.md
│   └── {custom}.md
└── rules/
    └── guardrails.json
```

### Вариант C: VS Code workspace settings

```
.vscode/
└── settings.json                  # OpenCode extension settings
```

## Конфигурация проекта

### Универсальный подход (рекомендуется)

Создать `.opencode/instructions.md` + набор prompt-файлов:

```markdown
# Project Guardrails — {ProjectName}

## Role
You are a .NET developer in the {ProjectName} project.

## Rules
- Do not add dependencies without explicit request
- Follow Clean Architecture / Vertical Slice / {arch}
- Every bug fix comes with a regression test
- ...

## Code Review Protocol
For any change:
1. Check diff for SQL injection
2. Check nullable reference types
3. Make sure CancellationToken is passed through
4. Check that there is no data leak

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...
```

### Если OpenCode поддерживает prompt-файлы:

```
.opencode/
├── instructions.md                # Base rules
└── prompts/
    ├── onboarding.md              # Project scanning
    ├── code-review.md             # PR review
    ├── security-audit.md          # Security audit
    ├── complexity-audit.md        # Complexity audit
    ├── allocation-budget-audit.md # Hot path allocation audit
    └── architecture-audit.md      # Architecture audit
```

## Запуск онбординга

Зависит от реализации OpenCode:

```bash
# If CLI
opencode --prompt .opencode/prompts/onboarding.md

# If VS Code extension
# Open Command Palette → OpenCode: Run Prompt → onboarding
```

## Что онбординг создаёт для код-ревью

**Цель:** после оценки проекта получить артефакт для код-ревью под вашу конфигурацию OpenCode, а не навязанный общий цикл.

### 1. Что решает онбординг

Онбординг должен определить:

- где в вашем форке хранить review-инструкции: в `AGENTS.md`, `.opencode/instructions.md` или prompt-файле
- нужен ли отдельный `code-review.md` prompt
- какие проверки нужно адаптировать под стек и ограничения конкретной модели

### 2. Что должно появиться в проекте

- `.opencode/prompts/code-review.md` или эквивалентный файл формата вашего форка
- базовые review-правила в `.opencode/instructions.md` и/или `AGENTS.md`
- явная фиксация, что было адаптировано, а что признано неприменимым

### 3. Когда сценарий считается успешным

- у команды есть один понятный запрос для ревью в этой конфигурации OpenCode
- ограничения и N/A-проверки, добавленные специально под проект, уже отражены в артефактах
- review не зависит от того, кто именно в команде сейчас помнит правильный промпт

### 4. Важная граница

Онбординг сначала формирует артефакт для ревью под вашу конфигурацию OpenCode. Уже потом этот артефакт используется на PR и при ручном ревью.

## Специфика OpenCode

### Что отличается от проприетарных агентов

| Аспект | Kimi | Claude Code | Codex | OpenCode |
|--------|------|-------------|-------|----------|
| Формат | `.kimi/skills/*.md` | `.claude/CLAUDE.md` | `.codex/instructions.md` | Зависит от реализации |
| Open Source | Нет | Нет | Нет | Да |
| Self-hosted | Нет | Нет | Нет | Возможно |
| Model choice | Фиксировано | Claude | GPT-4o | Любая (Ollama, etc.) |
| Интеграция | CLI | CLI + IDE | CLI | CLI + IDE |

### Нюансы OpenCode

1. **Нестандартный формат.** Поскольку OpenCode — open-source, разные форки могут иметь разные форматы конфигурации. Рекомендуется:
   - Использовать простой Markdown (универсально)
   - Держать instructions в одном месте
   - Документировать формат в `README.md` проекта

2. **Self-hosted models.** Если OpenCode работает с локальными моделями (Ollama, LM Studio):
   - Контекст может быть меньше (4k-32k tokens)
   - Инструкции должны быть короче и конкретнее
   - Нужно больше примеров (few-shot learning)

3. **Плагинная архитектура.** OpenCode может поддерживать плагины:
   - Можно написать плагин для запуска `dotnet test`
   - Можно написать плагин для NetArchTest
   - Можно написать плагин для verify-tests

## Рекомендуемый формат для OpenCode

Поскольку OpenCode не имеет единого стандарта, рекомендуется **гибридный подход**:

```
{project-root}/
├── AGENTS.md                      # Universal constitution (read by all agents)
├── CONVENTIONS.md                 # Naming, workflow
├── .opencode/
│   ├── instructions.md            # Brief instructions for OpenCode
│   └── prompts/                   # Prompt files for tasks
│       ├── onboarding.md
│       ├── code-review.md
│       └── security-audit.md
├── .kimi/skills/                  # If Kimi is used
├── .claude/                       # If Claude Code is used
└── .codex/instructions.md         # If Codex is used
```

### Универсальная конституция `AGENTS.md`

Используй [`rules/AGENTS_TEMPLATE.md`](../../rules/AGENTS_TEMPLATE.md) как базу. Для OpenCode важно не дублировать её в `.opencode/instructions.md`, а оставить там только специфичные для формата дополнения.

## Онбординг для OpenCode

Агент при онбординге:

1. Определяет, какой формат использует OpenCode в проекте (спрашивает или проверяет `.opencode/`)
2. Генерирует конфигурацию в правильном формате
3. Создаёт `AGENTS.md` (универсальный) + `.opencode/instructions.md` (специфичный)
4. Генерирует prompt-файлы для частых задач

## Ограничения

- Нет единого стандарта конфигурации
- Может требоваться адаптация под конкретный форк/версию
- Self-hosted модели могут плохо следовать длинным инструкциям
- Меньше встроенных tools по сравнению с Claude Code
