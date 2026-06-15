# .NET Skeptical AI Engineering

Методология ускорения инженерных практик через AI-агентов. Audit, review и guardrails, которые раньше требовали дорогой экспертизы, теперь масштабируются. Основана на докладе «ИИ уверен. Я нет» (Dotnext 2026).

[🇬🇧 English version](README.en.md)

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![License MIT](https://img.shields.io/badge/License-MIT-green.svg)
![CI](https://github.com/svetkis/dotnet-skeptical-ai/workflows/DemoProject%20CI/badge.svg)

> Хотя примеры и тесты реализованы на .NET, сама методология Decision Guards, трёхуровневых циклов проверок и гигиены промптов применима к любому стеку.

Репозиторий содержит готовые артефакты для .NET-проектов: правила, скиллы, тестовые паттерны и CI-воркфлоу.

## Проблема

AI-агенты (Cursor, Claude, Copilot) ускоряют написание кода, но генерируют скрытый техдолг, нарушают архитектурные границы и ломают безопасность. Ручное ревью такого кода становится бутылочным горлышком.

**Skeptical AI** — методология Zero-Trust к сгенерированному коду. Контроль переносится из вероятностных промптов в детерминированные пайплайны.

## Как это работает

Три контура проверки: внутренний (на каждом изменении), внешний (по расписанию или перед релизом) и груминг артефактов (раз в спринт).

### Слой 1. Цикл разработки (быстрая обратная связь)

| Подслой | Скорость | Инструмент |
|---------|----------|-----------|
| 0. Инструкции | — | `rules/AGENTS_TEMPLATE.md` + Decision Guards |
| 1.1 Компилятор | ~сек | `dotnet build`, `tsc --noEmit` |
| 1.2 Архитектура | ~10 сек | NetArchTest |
| 1.3 Тесты | ~30 сек | TUnit + `dotnet run` |
| 1.4 Code review | ~2 мин | Отдельный агент |
| 1.5 Smoke | ~5 мин | 10 критичных сценариев |

### Слой 2. Приёмочный цикл

| Подслой | Частота | Инструмент |
|---------|---------|-----------|
| 2.1 E2E MCP | Перед релизом | Telegram, browser, API тулы |
| 2.2 Аудиты | Раз в спринт / на PR в зоне риска | Security, DBA, UX, perf, i18n |
| 2.3 Нагрузка | Перед релизом | NBomber |

### Внешний цикл

| Уровень | Частота | Инструмент |
|---------|---------|-----------|
| Человек | После релиза | Бизнес- и продуктовые решения |

`templates/skills/` — готовые промпты для аудитов. Запускаются по расписанию или когда меняется код в зоне ответственности.

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
# Открой .agents/skills/skeptical-ai-bootstrap/SKILL.md — прогони чеклист,
# разберись что уже есть и что внедрить в первую очередь.

# 4. Адаптируй скиллы под свой стек
# См. templates/skills/ADAPTATION.md — вычеркни неприменимые проверки.

# 5. Скопируй нужные артефакты
cp rules/AGENTS_TEMPLATE.md /your/project/
cp -r templates/skills/code-review /your/project/.kimi/skills/
cp tests/patterns/*.cs /your/project/tests/
```

## Структура

```
.
├── AGENTS.md                     # Инструкции для AI-агентов
├── PYRAMID.md                    # Подробный разбор слоёв
├── rules/
│   ├── AGENTS_TEMPLATE.md        # Базовая конституция для агентов (универсальная)
│   ├── AGENTS_TEMPLATE.efcore.md # Add-on: EF Core-специфичные правила
│   ├── AGENTS_TEMPLATE.dapper.md # Add-on: Dapper / Raw SQL-специфичные правила
│   └── CONVENTIONS.md            # Коммиты, воркфлоу, тесты
├── templates/skills/                        # Роли агента
│   ├── memory-hygiene/            # Grooming: Auto Memory
│   ├── doc-hygiene/               # Grooming: документация
│   ├── backlog-hygiene/           # Grooming: бэклог
│   ├── skeptical-ai-bootstrap/    # Оценка зрелости + бэклог guardrails
│   ├── code-review/               # Inner loop: review на каждый PR
│   ├── task-compliance/           # Inner loop: проверка scope
│   ├── security-audit/            # Outer loop: по триггеру
│   ├── dba-audit/                 # Outer loop: по триггеру (EF Core)
│   ├── dba-audit-dapper/          # Outer loop: по триггеру (Dapper / Raw SQL)
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
    ├── DemoProject/               # Рабочий пример на .NET 10 (Clean Architecture)
    ├── DemoProject.MinimalApi/    # Single-project MVP (Minimal API, no layers)
    └── DemoProject.Traps/         # Intentionally broken code — демонстрация guardrails
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

## DemoProject.Traps

`examples/DemoProject.Traps/` — специально сломанный код для демонстрации guardrails в действии. Каждый тест здесь падает, показывая, что ловит архитектурный тест, если агент нарушает правила.

```bash
cd examples/DemoProject.Traps
dotnet run --project tests/DemoProject.Traps.Tests
```

**Что ломается:**
- `MutableState` — мутабельное состояние в Domain
- `DomainLeakingToInfra` — Domain зависит от `System.Net.Http`
- `PaymentService` — прямая зависимость между Features (Orders → Payments)
- `Modules/` — циклические зависимости между модулями (ArchUnitNET)
- `RawGuidEntity` — голый `Guid` вместо strongly typed ID

См. также [`examples/DemoProject.Traps/README.md`](examples/DemoProject.Traps/README.md).

## DemoProject.MinimalApi

`examples/DemoProject.MinimalApi/` — вариант для **Minimal API без Clean Architecture**. Показывает, как адаптировать guardrails, когда нет слоёв Domain / Application / Infrastructure.

```bash
cd examples/DemoProject.MinimalApi
dotnet build
dotnet run --project tests/DemoProject.MinimalApi.Tests
```

**Что внутри:**
- Naming conventions, banned APIs (`DateTime.Now`)
- `CancellationToken` guard
- Ratchet-тесты на публичные типы
- Duplication guard для бизнес-логики

См. также [`examples/DemoProject.MinimalApi/README.md`](examples/DemoProject.MinimalApi/README.md).

## Навигация

Потерялись? Начните с [docs/README.md](docs/README.md).

| Что нужно | Куда идти |
|-----------|-----------|
| Правила для агента (базовые) | `rules/AGENTS_TEMPLATE.md` |
| EF Core add-on | `rules/AGENTS_TEMPLATE.efcore.md` |
| Dapper add-on | `rules/AGENTS_TEMPLATE.dapper.md` |
| Аудит безопасности | `templates/skills/security-audit/` |
| Аудит БД (EF Core) | `templates/skills/dba-audit/` |
| Аудит БД (Dapper) | `templates/skills/dba-audit-dapper/` |
| Аудит производительности | `templates/skills/performance-audit/` |
| Аудит локализации | `templates/skills/i18n-audit/` |
| Code review агент | `templates/skills/code-review/` |
| Проверка scope | `templates/skills/task-compliance/` |
| Паттерн теста | `tests/patterns/` |
| CI безопасность | `ci/github-actions/safe-ci.yml` |
| Ловушки агента | `docs/traps/` |
| Груминг Auto Memory | `templates/skills/memory-hygiene/` |
| Груминг доков | `templates/skills/doc-hygiene/` |
| Груминг бэклога | `templates/skills/backlog-hygiene/` |
| Архитектурные тесты | `docs/solutions/architecture-tests.md` |
| Паттерны AI-разработки | `docs/solutions/ai-patterns.md` |
| Онбординг проекта | `templates/skills/skeptical-ai-bootstrap/` |
| Рабочий пример (Clean Architecture) | `examples/DemoProject/` |
| Рабочий пример (Single-project MVP) | `examples/DemoProject.MinimalApi/` |
| Failing demo (guardrails) | `examples/DemoProject.Traps/` |
| Интеграция с Kimi | `docs/agents/KIMI.md` |
| Интеграция с Claude Code | `docs/agents/CLAUDE-CODE.md` |
| Интеграция с Cursor | `docs/agents/CURSOR.md` |
| Интеграция с Codex | `docs/agents/CODEX.md` |
| Интеграция с OpenCode | `docs/agents/OPENCODE.md` |
| Bootstrap Protocol | `docs/agents/BOOTSTRAP-PROTOCOL.md` |
| Сравнение агентов | `docs/agents/README.md` |

## Автор

**Светлана Мелешкина** — автор методологии Skeptical AI Engineering, докладчик Dotnext 2026.

- 💬 Telegram-канал: [@kot_review](https://t.me/kot_review)
- ✉️ Telegram: [@svetkis](https://t.me/svetkis)

## Контрибуция

См. [CONTRIBUTING.md](CONTRIBUTING.md). Принимаем новые скиллы, паттерны тестов, ловушки и интеграции с агентами.

## Лицензия

[MIT](LICENSE)
