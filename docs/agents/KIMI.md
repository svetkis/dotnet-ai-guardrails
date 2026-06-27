# Kimi Code CLI — Guardrails Integration

> Kimi Code CLI использует систему скиллов в `.kimi/skills/`.
> Это нативный формат для репозитория `dotnet-ai-guardrails`.

## Структура интеграции

```
.kimi/
└── skills/
    ├── skeptical-ai-bootstrap/          # Scanning + backlog
    ├── code-review/                 # Review on every PR / pre-commit (.NET)
    ├── frontend-code-review/        # Review on every PR / pre-commit (React + TS)
    ├── task-compliance/             # Scope check
    ├── security-audit/              # Security audit
    ├── dba-audit/                   # DB audit
    ├── performance-audit/           # Performance audit
    ├── api-design-audit/            # API design audit
    ├── bot-audit/                   # Telegram bot audit
    ├── i18n-audit/                  # Localization audit
    ├── complexity-audit/            # Method complexity audit
    ├── allocation-budget-audit/     # Hot path allocation audit
    ├── spellcheck-audit/            # Spellcheck audit
    ├── release-readiness-audit/     # Release readiness audit
    ├── mutation-audit/              # Mutation testing audit
    ├── analyzer-tests-audit/        # Roslyn analyzer tests audit
    └── {project-specific}/          # Custom skills
```

## Конфигурация проекта

Создать `.kimi/skills/README.md` (генерируется онбордингом):
- Карта всех скиллов
- Статусы Active / WIP / Backlog
- Интеграционная схема

## Запуск онбординга

```bash
# Install the onboarding skill
mkdir -p ./.kimi/skills/skeptical-ai-bootstrap
cp /path/to/dotnet-ai-guardrails/.agents/skills/skeptical-ai-bootstrap/SKILL.md ./.kimi/skills/skeptical-ai-bootstrap/
cp -r /path/to/dotnet-ai-guardrails/templates/skills/skeptical-ai-bootstrap/* ./.kimi/skills/skeptical-ai-bootstrap/

# Run
kimi run skeptical-ai-bootstrap
```

## Что онбординг создаёт для код-ревью

**Цель:** после первого сканирования получить не абстрактную схему проверки, а скилл ревью под ваш стек.

### 1. Что решает онбординг

Онбординг должен определить один из трёх исходов:

- готовый `code-review` подходит и его можно адаптировать по именам и соглашениям проекта
- готовый `code-review` не подходит целиком, но его можно существенно переписать под стек
- нужен новый скилл ревью: например `code-review-dapper`, `code-review-razor`, `code-review-netframework`

### 2. Что должно появиться в проекте

- скилл ревью в `.kimi/skills/code-review/` или `.kimi/skills/code-review-{context}/`
- зафиксированная причина, почему взяли стандартный скилл или почему создали новый
- обновлённая карта `.kimi/skills/README.md`, если вы её ведёте через onboarding

### 3. Когда сценарий считается успешным

- команда знает точное имя скилла ревью для PR/commit-проверки
- в скилле уже убраны очевидные ложные срабатывания под ваш стек
- если стандартный `code-review` не подошёл, это отражено в отчёте, а не потеряно в устной договорённости

### 4. Важная граница

Сначала онбординг определяет и адаптирует скилл ревью под проект. Только потом этот скилл начинает жить в pre-commit / PR-потоке.

## Конкретные команды (ежедневный workflow)

> **Примечание:** Флаги (`--git-diff`, `--paths`, `--mode`) — это **псевдокоманды** для иллюстрации workflow.
> Kimi Code CLI может не поддерживать их нативно. Адаптируйте под реальный синтаксис своей версии Kimi
> (например, передавайте diff через stdin или используйте переменные окружения).

### Code review перед коммитом

Скилл `code-review` заточен под pre-commit: он читает `git diff --cached` и срабатывает
автоматически перед коммитом (через триггер в `SKILL.md`) или явно через `/skill:code-review`.

```bash
# Review staged changes before commit
# (the skill reads git diff --cached itself)
kimi run code-review

# Review the last commit
kimi run code-review --git-diff HEAD~1

# Review current unstaged changes
kimi run code-review --git-diff

# Review a PR (branch vs main)
kimi run code-review --git-diff main...feature/my-branch
```

### Аудиты по расписанию

```bash
# Security audit on diff
kimi run security-audit --git-diff HEAD~5

# DBA audit when migrations change
kimi run dba-audit --git-diff --paths "src/*/Infrastructure/Migrations/"

# Performance audit before release
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
# Copy all audit skills
for skill in code-review task-compliance security-audit dba-audit performance-audit; do
    cp -r /path/to/dotnet-ai-guardrails/templates/skills/$skill ./.kimi/skills/
done

# Generate skills README
kimi skills list
```

## Структура `.kimi/skills/README.md` (рекомендуется)

```markdown
# Project Skills

## Inner Loop (on every PR)
- `code-review` — diff-based review
- `task-compliance` — scope check

## Outer Loop (audits)
- `security-audit` — once per sprint
- `dba-audit` — on migrations
- `dba-audit-dapper` — if the stack is Dapper

## Grooming (once per sprint)
- `memory-hygiene`
- `doc-hygiene`

## Statuses
| Skill | Status | Adapted |
|-------|--------|---------|
| code-review | Active | ✅ |
| dba-audit | Backlog | ❌ (we use Dapper) |
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
