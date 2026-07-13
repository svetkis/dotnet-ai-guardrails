# Глоссарий

> Ключевые термины репозитория. Если вы встретили незнакомое слово в `AGENTS.md` или `PYRAMID.md` — скорее всего, оно здесь.
>
> [🇬🇧 English version](GLOSSARY.en.md)

---

## Engineering Assurance Levels (модель контроля)

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Control Foundation** | Основание контроля: `AGENTS.md`, архитектурные границы, Decision Guards, policies. Действует до изменения кода. | [README.md](README.md#как-это-работает), [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **1. Change Checks** | Проверки изменения: компилятор, nullable, analyzers, formatting, banned APIs, dependency checks, pre-commit review. IDE, build, pre-commit. | [README.md](README.md#как-это-работает) |
| **2. Behavior Checks** | Проверки поведения: unit, regression, contract, characterization, architecture tests, ratchets. Локальный или CI test run. | [README.md](README.md#как-это-работает), [tests/patterns/](tests/patterns/) |
| **3. System Checks** | Проверки системы целиком: integration, E2E, smoke, Testcontainers, load, deployment verification. PR, CI, release pipeline. | [README.md](README.md#как-это-работает) |
| **4. Periodic Assurance** | Исследовательские аудиты по расписанию или risk-trigger: security, database, performance, UX, API, i18n, tech-debt. | `templates/skills/*-audit/` |
| **Engineering Governance** | Процесс (не уровень): принятие остаточного риска, release decision, бизнес- и продуктовые решения. | [docs/solutions/human-audit-bridge.md](docs/solutions/human-audit-bridge.md) |
| **Control Maintenance** | Процесс (не уровень): актуализация инструкций, agent memory, backlog, baselines, suppressions, guardrails. | `templates/skills/memory-hygiene/`, `doc-hygiene/`, `backlog-hygiene/` |
| **AGENTS.md** | Файл с правилами для AI-агентов. Читается агентом перед каждой задачей. Может быть иерархическим (корневой + по модулям). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Слои 0–2 / Inner-Outer loop** | *Legacy:* имена из визуальной метафоры доклада (PYRAMID.md). Маппинг на уровни — в начале [PYRAMID.md](PYRAMID.md). | [PYRAMID.md](PYRAMID.md) |

## Тестовые паттерны

| Термин | Определение | Пример |
|--------|-------------|--------|
| **Ratchet** | Инвентаризация тестами: метрика (например, количество публичных типов или тестов) должна **не уменьшаться**. Если агент удаляет типы или тесты — тест падает. | [tests/patterns/RatchetTest.cs](tests/patterns/RatchetTest.cs) |
| **BUG###** | Соглашение об именовании regression-тестов: один баг = один файл `BUG###_DescriptiveName.cs`. Покрывает все пути, по которым баг мог проявиться. | [tests/conventions/BUG_TEMPLATE.cs](tests/conventions/BUG_TEMPLATE.cs) |
| **Snapshot test** | Тест, который захватывает и сравнивает вывод (JSON, OpenAPI) с эталонным файлом. Если DTO меняется — snapshot ломается. | [tests/patterns/SnapshotTest.cs](tests/patterns/SnapshotTest.cs) |
| **Characterization test** | Тест, который фиксирует текущее поведение системы без оценки корректности. Нужен, чтобы рефакторинг не менял поведение. | [PYRAMID.md §Слой 1.3](PYRAMID.md#layer-1-tests) |
| **«0 tests ran»** | Проблема, когда раннер не нашёл тестов, но exit code = 0. CI выглядит зелёным, хотя ничего не проверено. | [PYRAMID.md §Слой 1.3](PYRAMID.md#layer-1-tests) |

## Паттерны кода

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Read-path** | Путь чтения данных: read-only запросы. `.Select()` + `.AsNoTracking()` **обязательны**. `.Include()`, `.FindAsync()` запрещены. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Write-path** | Путь записи данных: команды, изменяющие состояние. Change tracking обязателен, `.AsNoTracking()` запрещён. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Decision Guard** | SAE-specific: осознанное отклонение от стандарта, зафиксированное ID в комментарии (`PERF-###`, `DB-###`, `AUD-###`) и короткой записью в реестре. **Не синоним ADR** — это лёгкая ссылка на решение; при наличии полноценных ADR запись ссылается на них. Проверяется архитектурным тестом на уникальность. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md), [templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md](templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) |
| **Semantic Anchors** | Устоявшиеся термины вместо описаний. Каждый термин активирует конкретную методологию (например, "read-path" = `.Select()` + `.AsNoTracking()`). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Агенты и инструменты

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **MCP (Model Context Protocol)** | Протокол подключения внешних инструментов к AI-агенту. Позволяет агенту "протыкивать" Telegram, браузер, API. | [PYRAMID.md §Слой 2.1](PYRAMID.md#layer-2-e2e) |
| **Code Review Agent** | Отдельный инстанс AI-агента, который ревьюит diff **до** коммита. Не тот, кто писал код. | [templates/skills/code-review/SKILL.md](templates/skills/code-review/SKILL.md) |
| **Skill** | Роль агента: инструкция + чеклист для конкретной задачи (аудит, ревью, онбординг). Устанавливается в `.kimi/skills/` или аналог. | `templates/skills/` |
| **Context Marker** | Emoji-маркер в начале ответа агента, показывающий активный контекст: 🍀 (базовые правила), 🔍 (ревью), ✅ (коммит). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Focused Agent** | Принцип: один агент — одна задача. Агент-ревьюер не пишет код; агент-кодер не ревьюит. | [templates/skills/code-review/SKILL.md](templates/skills/code-review/SKILL.md) |

## Процессы и метрики

| Термин | Определение | Где используется |
|--------|-------------|------------------|
| **Audit** | Глубокая проверка одной узкой области (security, perf, DB). Запускается раз в спринт или по триггеру, не на каждый PR. | `templates/skills/` |
| **Cross-pollination** | Обмен находками между аудитами. Например, security-аудит находит утечку в логах, а UX-аудит обнаруживает тот же endpoint как тупиковый. | [PYRAMID.md §Внешний цикл](PYRAMID.md#outer-loop) |
| **P50 / P95 / Max** | Персентили latency: медиана, 95-й перцентиль, максимум. Агенты часто оптимизируют P50, забывая про tail latency (Max). | [docs/traps/p50-vs-max.md](docs/traps/p50-vs-max.md) |
| **Scope creep** | Расширение задачи: агент добавляет в PR изменения, выходящие за рамки исходного запроса. | [templates/skills/task-compliance/SKILL.md](templates/skills/task-compliance/SKILL.md) |
| **Silent misalignment** | Безмолвная ошибка: агент не задаёт уточняющих вопросов, хотя инструкции неясны или противоречивы. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Технологии

| Термин | Определение | Ссылка |
|--------|-------------|--------|
| **TUnit** | Современный тестовый фреймворк для .NET. Используется в этом репозитории вместо xUnit/NUnit. Запуск через `dotnet run --project`. | [tests/conventions/TUnit_Guide.md](tests/conventions/TUnit_Guide.md) |
| **NetArchTest** | Библиотека для архитектурных тестов на основе рефлексии. Проверяет зависимости между слоями, именование, интерфейсы. | [tests/patterns/ArchitectureRules.cs](tests/patterns/ArchitectureRules.cs) |
| **NBomber** | Фреймворк нагрузочного тестирования. Ловит silent breakdown и слабые места под смешанной нагрузкой read+write, а не просто "деградацию под высокой нагрузкой". | [tests/patterns/LoadTest.cs](tests/patterns/LoadTest.cs) |
| **Testcontainers** | Инфраструктура для запуска реальных БД (PostgreSQL, Redis) в Docker-контейнерах во время тестов. Альтернатива EF Core InMemory-провайдеру. | [docs/traps/silent-breakdown.md](docs/traps/silent-breakdown.md) |
