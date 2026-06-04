# Глоссарий / Glossary

> Ключевые термины репозитория. Если вы встретили незнакомое слово в `AGENTS.md` или `PYRAMID.md` — оно скорее всего здесь.

---

## Архитектура обратной связи

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Слой 1 (цикл разработки)** | Быстрая обратная связь: Компилятор → Архитектура → Тесты → Code Review → Smoke. Бегут на каждом изменении, от секунд до минут. | [PYRAMID.md §Слой 1](PYRAMID.md#layer-1) |
| **Внешний цикл (Outer loop)** | Окончательная проверка человеком, бизнес- и продуктовые решения. Не часть ежедневного feedback loop. | [PYRAMID.md §Внешний цикл](PYRAMID.md#outer-loop) |
| **Слой 0** | Инструкции для агента: `AGENTS.md` + нумерованные решения. Агент читает перед кодом. | [PYRAMID.md §Слой 0](PYRAMID.md#layer-0) |
| **AGENTS.md** | Файл с правилами для AI-агентов. Читается агентом перед каждой задачей. Может быть иерархическим (корневой + per-module). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Тестовые паттерны

| Термин | Определение | Пример |
|--------|-------------|--------|
| **Ratchet** | Тест-инвентарь: метрика (например, количество публичных типов или тестов) должна **не уменьшаться**. Если агент удалил типы или тесты — тест падает. | [tests/patterns/RatchetTest.cs](tests/patterns/RatchetTest.cs) |
| **BUG###** | Конвенция именования regression-тестов: один баг = один файл `BUG###_DescriptiveName.cs`. Покрывает все code paths, где баг мог проявиться. | [tests/conventions/BUG_TEMPLATE.cs](tests/conventions/BUG_TEMPLATE.cs) |
| **Snapshot-тест** | Тест, который фиксирует и сравнивает выход (JSON, OpenAPI) с эталонным файлом. Если DTO изменился — snapshot ломается. | [tests/patterns/SnapshotTest.cs](tests/patterns/SnapshotTest.cs) |
| **Characterization-тест** | Тест, который фиксирует текущее поведение системы без оценки его корректности. Нужен, чтобы рефакторинг не менял поведение. | [PYRAMID.md §Слой 1.3](PYRAMID.md#layer-1-tests) |
| **"0 tests ran"** | Проблема, когда тестовый раннер не нашёл тестов, но exit code = 0. CI выглядит зелёным, хотя ничего не проверено. | [PYRAMID.md §Слой 1.3](PYRAMID.md#layer-1-tests) |

## Паттерны кода

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Read-path** | Путь чтения данных: запросы только на чтение. **Обязательны** `.Select()` + `.AsNoTracking()`. Запрещены `.Include()`, `.FindAsync()`. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Write-path** | Путь записи данных: команды, изменяющие состояние. Требуется change tracking, запрещён `.AsNoTracking()`. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Numbered Decision (ADR)** | Осознанное отклонение от стандарта, задокументированное ID в комментарии: `PERF-###`, `DB-###`, `AUD-###`. Реализация паттерна **ADR (Architecture Decision Records)**. Проверяется архитектурным тестом на уникальность. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md), [tests/patterns/ArchitectureRules.cs](tests/patterns/ArchitectureRules.cs) |
| **Semantic Anchors** | Установленные термины вместо описаний. Каждый термин активирует конкретную методологию (например, "read-path" = `.Select()` + `.AsNoTracking()`). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Агенты и инструменты

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **MCP (Model Context Protocol)** | Протокол для подключения внешних инструментов к AI-агенту. Позволяет агенту "тыкать" Telegram, браузер, API. | [PYRAMID.md §Слой 2.1](PYRAMID.md#layer-2-e2e) |
| **Code Review агент** | Отдельный экземпляр AI-агента, который ревьюит diff **до** коммита. Не тот, что писал код. | [skills/code-review/SKILL.md](skills/code-review/SKILL.md) |
| **Skill** | Роль агента: инструкция + чеклист для конкретной задачи (аудит, ревью, онбординг). Устанавливается в `.kimi/skills/` или аналог. | `skills/` |
| **Context Marker** | Эмодзи-маркер в начале ответа агента, показывающий активный контекст: 🍀 (ground rules), 🔍 (review), ✅ (commit). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Focused Agent** | Принцип: один агент — одна задача. Review-агент не пишет код, code-агент не ревьюит. | [skills/code-review/SKILL.md](skills/code-review/SKILL.md) |

## Процессы и метрики

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Аудит (Audit)** | Глубинная проверка одной узкой области (security, perf, БД). Запускается раз в спринт или по триггеру, а не на каждый PR. | `skills/` |
| **Cross-pollination** | Обмен находками между аудитами. Например, security-аудит находит лог-утечку, UX-аудит находит тот же endpoint как dead-end. | [PYRAMID.md §Внешний цикл](PYRAMID.md#outer-loop) |
| **P50 / P95 / Max** | Персентили latency: медиана, 95-й перцентиль, максимум. Агенты часто оптимизируют P50, забывая про tail latency (Max). | [docs/traps/p50-vs-max.md](docs/traps/p50-vs-max.md) |
| **Scope creep** | Расползание задачи: агент добавляет в PR изменения, выходящие за рамки исходного запроса. | [skills/task-compliance/SKILL.md](skills/task-compliance/SKILL.md) |
| **Silent misalignment** | Молчаливая ошибка: агент не задал уточняющих вопросов, хотя инструкции были неясны или противоречивы. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Технологии

| Термин | Определение | Ссылка |
|--------|-------------|--------|
| **TUnit** | Современный тестовый фреймворк для .NET. Используется в этом репозитории вместо xUnit/NUnit. Запуск через `dotnet run --project`. | [tests/conventions/TUnit_Guide.md](tests/conventions/TUnit_Guide.md) |
| **NetArchTest** | Библиотека для архитектурных тестов на базе рефлексии. Проверяет зависимости между слоями, именование, интерфейсы. | [tests/patterns/ArchitectureRules.cs](tests/patterns/ArchitectureRules.cs) |
| **NBomber** | Фреймворк для нагрузочного тестирования. Ловит silent breakdown и слабые места кода при смешанной нагрузке read+write, а не только «деградацию на хайлоаде». | [tests/patterns/LoadTest.cs](tests/patterns/LoadTest.cs) |
| **Testcontainers** | Инфраструктура для запуска реальных БД (PostgreSQL, Redis) в Docker-контейнерах во время тестов. Альтернатива InMemory-провайдеру EF Core. | [docs/traps/silent-breakdown.md](docs/traps/silent-breakdown.md) |
