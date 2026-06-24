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
├── config.json                    # Настройки
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
└── settings.json                  # Настройки OpenCode extension
```

## Конфигурация проекта

### Универсальный подход (рекомендуется)

Создать `.opencode/instructions.md` + набор prompt-файлов:

```markdown
# Project Guardrails — {ProjectName}

## Роль
Ты — .NET-разработчик в проекте {ProjectName}.

## Правила
- Не добавляй зависимости без explicit запроса
- Следуй Clean Architecture / Vertical Slice / {arch}
- Каждый баг-фикс — с regression тестом
- ...

## Code Review Protocol
При любом изменении:
1. Проверь diff на SQL injection
2. Проверь nullable reference types
3. Убедись, что CancellationToken прокидывается
4. Проверь, что нет утечки данных

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...
```

### Если OpenCode поддерживает prompt-файлы:

```
.opencode/
├── instructions.md                # Базовые правила
└── prompts/
    ├── onboarding.md              # Сканирование проекта
    ├── code-review.md             # Review PR
    ├── security-audit.md          # Аудит безопасности
    ├── complexity-audit.md        # Аудит сложности
    ├── allocation-budget-audit.md # Аудит аллокаций hot path
    └── architecture-audit.md      # Аудит архитектуры
```

## Запуск онбординга

Зависит от реализации OpenCode:

```bash
# Если CLI
opencode --prompt .opencode/prompts/onboarding.md

# Если VS Code extension
# Открыть Command Palette → OpenCode: Run Prompt → onboarding
```

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
├── AGENTS.md                      # Универсальная конституция (читается всеми агентами)
├── CONVENTIONS.md                 # Naming, workflow
├── .opencode/
│   ├── instructions.md            # Краткие инструкции для OpenCode
│   └── prompts/                   # Prompt-файлы для задач
│       ├── onboarding.md
│       ├── code-review.md
│       └── security-audit.md
├── .kimi/skills/                  # Если используется Kimi
├── .claude/                       # Если используется Claude Code
└── .codex/instructions.md         # Если используется Codex
```

### Универсальная конституция `AGENTS.md`

```markdown
# AGENTS.md — {ProjectName}

> Этот файл читается ЛЮБЫМ AI-агентом, работающим в проекте.
> Формат: Markdown, независимый от инструмента.

## Правила (универсальные)
1. Не добавляй зависимости без explicit запроса
2. Не меняй структуру папок
3. Не удаляй тесты
4. Каждый баг-фикс — с regression тестом `BUG###_`

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}

## Архитектура
- {Clean / Vertical Slice / etc.}
- Границы слоёв: ...

## Conventions
- ...
```

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
