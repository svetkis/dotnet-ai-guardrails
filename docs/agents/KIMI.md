# Kimi Code CLI — Guardrails Integration

> Kimi Code CLI использует систему скиллов в `.kimi/skills/`.
> Это нативный формат для репозитория `dotnet-skeptical-ai`.

## Структура интеграции

```
.kimi/
└── skills/
    ├── skeptical-ai-bootstrap/          # Сканирование + бэклог
    ├── code-review/                 # Review на каждый PR / pre-commit (.NET)
    ├── frontend-code-review/        # Review на каждый PR / pre-commit (React + TS)
    ├── task-compliance/             # Проверка scope
    ├── security-audit/              # Аудит безопасности
    ├── dba-audit/                   # Аудит БД
    ├── performance-audit/           # Аудит производительности
    ├── api-design-audit/            # Аудит дизайна API
    ├── bot-audit/                   # Аудит Telegram-ботов
    ├── i18n-audit/                  # Аудит локализации
    └── {project-specific}/          # Кастомные скиллы
```

## Конфигурация проекта

Создать `.kimi/skills/README.md` (генерируется онбордингом):
- Карта всех скиллов
- Статусы Active / WIP / Backlog
- Интеграционная схема

## Запуск онбординга

```bash
# Установить скилл онбординга
mkdir -p ./.kimi/skills/skeptical-ai-bootstrap
cp /path/to/dotnet-skeptical-ai/.agents/skills/skeptical-ai-bootstrap/SKILL.md ./.kimi/skills/skeptical-ai-bootstrap/
cp -r /path/to/dotnet-skeptical-ai/templates/skills/skeptical-ai-bootstrap/* ./.kimi/skills/skeptical-ai-bootstrap/

# Запустить
kimi run skeptical-ai-bootstrap
```

## Конкретные команды (ежедневный workflow)

> **Примечание:** Флаги (`--git-diff`, `--paths`, `--mode`) — это **псевдокоманды** для иллюстрации workflow.
> Kimi Code CLI может не поддерживать их нативно. Адаптируйте под реальный синтаксис своей версии Kimi
> (например, передавайте diff через stdin или используйте переменные окружения).

### Code review перед коммитом

Скилл `code-review` заточен под pre-commit: он читает `git diff --cached` и срабатывает
автоматически перед коммитом (через триггер в `SKILL.md`) или явно через `/skill:code-review`.

```bash
# Review staged-изменений перед коммитом
# (скилл сам читает git diff --cached)
kimi run code-review

# Review последнего коммита
kimi run code-review --git-diff HEAD~1

# Review текущих unstaged изменений
kimi run code-review --git-diff

# Review PR (ветка vs main)
kimi run code-review --git-diff main...feature/my-branch
```

### Аудиты по расписанию

```bash
# Security audit на diff
kimi run security-audit --git-diff HEAD~5

# DBA audit при изменениях в миграциях
kimi run dba-audit --git-diff --paths "src/*/Infrastructure/Migrations/"

# Performance audit перед релизом
kimi run performance-audit --mode pre-release
```

### Груминг артефактов (раз в спринт)

```bash
kimi run memory-hygiene
kimi run doc-hygiene
kimi run backlog-hygiene
```

### Установка всех скиллов разом

```bash
# Скопировать все аудит-скиллы
for skill in code-review task-compliance security-audit dba-audit performance-audit; do
    cp -r /path/to/dotnet-skeptical-ai/templates/skills/$skill ./.kimi/skills/
done

# Сгенерировать README скиллов
kimi skills list
```

## Структура `.kimi/skills/README.md` (рекомендуется)

```markdown
# Project Skills

## Inner Loop (на каждый PR)
- `code-review` — diff-based review
- `task-compliance` — проверка scope

## Outer Loop (аудиты)
- `security-audit` — раз в спринт
- `dba-audit` — при миграциях
- `dba-audit-dapper` — если стек Dapper

## Grooming (раз в спринт)
- `memory-hygiene`
- `doc-hygiene`

## Статусы
| Скилл | Статус | Адаптирован |
|-------|--------|-------------|
| code-review | Active | ✅ |
| dba-audit | Backlog | ❌ (у нас Dapper) |
```

## Нюансы

- Скиллы — это Markdown-файлы с YAML frontmatter
- Агент читает `SKILL.md` и применяет инструкции
- Custom skills создаются по шаблону `NEW-SKILL-TEMPLATE.md`
- Интеграция с CI через `kimi run {skill-name}` или ручной запуск

## Ограничения

- Kimi должен быть установлен локально (`kimi` CLI)
- Скиллы не автозапускаются на PR — нужен CI-обёртка или ручной запуск
- Контекст ограничен окном модели (~200k tokens)
