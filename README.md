# .NET Skeptical AI Engineering

Методология контроля качества кода при работе с AI-агентами. Основана на докладе «ИИ уверен. Я нет» (Dotnext 2026).

[🇬🇧 English version](README.en.md)

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![License MIT](https://img.shields.io/badge/License-MIT-green.svg)
![CI](https://github.com/svetkis/dotnet-skeptical-ai/workflows/DemoProject%20CI/badge.svg)

Репозиторий содержит готовые артефакты для .NET-проектов: правила, скиллы, тестовые паттерны и CI-воркфлоу.

## Как это работает

Три контура проверки: внутренний (на каждом изменении), внешний (по расписанию или перед релизом) и груминг артефактов (раз в спринт).

### Внутренний цикл — 5 слоёв

| Слой | Скорость | Инструмент |
|------|----------|-----------|
| 0. Инструкции | — | `rules/AGENTS_TEMPLATE.md` + Decision Guards |
| 1. Компилятор | ~сек | `dotnet build`, `tsc --noEmit` |
| 2. Архитектура | ~10 сек | NetArchTest |
| 3. Тесты | ~30 сек | TUnit + `dotnet run` |
| 4. Code review | ~2 мин | Отдельный агент |
| 5. E2E MCP | ~15 мин | Telegram, browser, API тулы |

### Внешний цикл — 3 уровня

| Уровень | Частота | Инструмент |
|---------|---------|-----------|
| 1. Аудиты | Раз в спринт / на PR в зоне риска | Security, DBA, UX, perf, i18n |
| 2. Нагрузка | Перед релизом | NBomber |
| 3. Ручное | После релиза | Человек |

`skills/` — готовые промпты для аудитов. Запускаются по расписанию или когда меняется код в зоне ответственности.

### Цикл груминга

Артефакты агента устаревают: AGENTS.md отстаёт от кода, Auto Memory накапливает дубли, бэклог превращается в кладбище.

| Скилл | Что чистит | Периодичность |
|-------|-----------|---------------|
| memory-hygiene | Auto Memory: дубли, drift, stale refs | Раз в спринт |
| doc-hygiene | AGENTS.md: консистентность иерархии, code drift | Раз в спринт |
| backlog-hygiene | Бэклог: stale, orphaned, duplicates | Раз в спринт |

## Быстрый старт

```bash
# 1. Клонируй
git clone https://github.com/svetkis/dotnet-skeptical-ai.git

# 2. Запусти DemoProject
cd examples/DemoProject
dotnet build
dotnet run --project tests/DemoProject.Tests

# 3. Оцени свой проект
# Открой skills/skeptical-ai-bootstrap/SKILL.md — прогони чеклист,
# разберись что уже есть и что внедрить в первую очередь.

# 4. Адаптируй скиллы под свой стек
# См. skills/ADAPTATION.md — вычеркни неприменимые проверки.

# 5. Скопируй нужные артефакты
cp rules/AGENTS_TEMPLATE.md /your/project/
cp -r skills/code-review /your/project/.kimi/skills/
cp tests/patterns/*.cs /your/project/tests/
```

## Структура

```
.
├── AGENTS.md                     # Инструкции для AI-агентов
├── PYRAMID.md                    # Подробный разбор слоёв
├── rules/
│   ├── AGENTS.md                 # EF-правила, naming, конвенции
│   └── CONVENTIONS.md            # Коммиты, воркфлоу, тесты
├── skills/                        # Роли агента
│   ├── memory-hygiene/            # Grooming: Auto Memory
│   ├── doc-hygiene/               # Grooming: документация
│   ├── backlog-hygiene/           # Grooming: бэклог
│   ├── skeptical-ai-bootstrap/    # Оценка зрелости + бэклог guardrails
│   ├── code-review/               # Inner loop: review на каждый PR
│   ├── task-compliance/           # Inner loop: проверка scope
│   ├── security-audit/            # Outer loop: по триггеру
│   ├── dba-audit/                 # Outer loop: по триггеру
│   ├── api-design-audit/          # Outer loop: по триггеру
│   ├── bot-audit/                 # Outer loop: по триггеру
│   ├── performance-audit/         # Outer loop: по триггеру
│   └── i18n-audit/                # Outer loop: по триггеру
├── docs/
│   ├── traps/                     # Ловушки агента
│   └── solutions/
│       ├── architecture-tests.md  # Гайд по arch-тестам
│       └── ai-patterns.md         # 10 паттернов AI-driven разработки
├── tests/
│   ├── patterns/                  # Шаблоны тестов (Ratchet, NetArchTest, NBomber)
│   └── conventions/               # Именование, TUnit гайд
├── ci/                            # CI/CD guardrails
└── examples/
    └── DemoProject/               # Рабочий пример на .NET 10
```

## DemoProject

`examples/DemoProject/` — рабочий пример на .NET 10 со всеми паттернами:

- Clean Architecture (Domain → Application → Infrastructure)
- NetArchTest: проверка зависимостей между слоями
- Ratchet-тесты: контроль публичных типов и количества тестов
- Snapshot-тесты: контракты JSON-сериализации
- NBomber: нагрузочные тесты (read + write mix)
- TUnit: запуск через `dotnet run --project`

```bash
cd examples/DemoProject
dotnet build
dotnet run --project tests/DemoProject.Tests
```

## Навигация

Потерялись? Начните с [docs/README.md](docs/README.md).

| Что нужно | Куда идти |
|-----------|-----------|
| Правила для агента | `rules/AGENTS_TEMPLATE.md` |
| Аудит безопасности | `skills/security-audit/` |
| Аудит БД | `skills/dba-audit/` |
| Аудит производительности | `skills/performance-audit/` |
| Аудит локализации | `skills/i18n-audit/` |
| Code review агент | `skills/code-review/` |
| Проверка scope | `skills/task-compliance/` |
| Паттерн теста | `tests/patterns/` |
| CI безопасность | `ci/github-actions/safe-ci.yml` |
| Ловушки агента | `docs/traps/` |
| Груминг Auto Memory | `skills/memory-hygiene/` |
| Груминг доков | `skills/doc-hygiene/` |
| Груминг бэклога | `skills/backlog-hygiene/` |
| Архитектурные тесты | `docs/solutions/architecture-tests.md` |
| Паттерны AI-разработки | `docs/solutions/ai-patterns.md` |
| Онбординг проекта | `skills/skeptical-ai-bootstrap/` |
| Интеграция с Kimi | `docs/agents/KIMI.md` |
| Интеграция с Claude Code | `docs/agents/CLAUDE-CODE.md` |
| Интеграция с Codex | `docs/agents/CODEX.md` |
| Интеграция с OpenCode | `docs/agents/OPENCODE.md` |
| Сравнение агентов | `docs/agents/README.md` |

## Автор

**Светлана Мелешкина** — автор методологии Skeptical AI Engineering, докладчик Dotnext 2026.

- 💬 Telegram-канал: [@kot_review](https://t.me/kot_review)
- ✉️ Telegram: [@svetkis](https://t.me/svetkis)

## Контрибуция

См. [CONTRIBUTING.md](CONTRIBUTING.md). Принимаем новые скиллы, паттерны тестов, ловушки и интеграции с агентами.

## Лицензия

[MIT](LICENSE)
