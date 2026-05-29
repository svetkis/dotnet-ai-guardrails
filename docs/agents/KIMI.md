# Kimi Code CLI — Guardrails Integration

> Kimi Code CLI использует систему скиллов в `.kimi/skills/`.
> Это нативный формат для репозитория `dotnet-agentic-engineering`.

## Структура интеграции

```
.kimi/
└── skills/
    ├── skeptical-ai-bootstrap/          # Сканирование + бэклог
    ├── code-review/                 # Review на каждый PR
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
cp -r /path/to/dotnet-skeptical-ai/skills/skeptical-ai-bootstrap ./.kimi/skills/

# Запустить
kimi run skeptical-ai-bootstrap
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
