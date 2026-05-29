# Пример отчёта онбординга (принципиальный подход)

> Этот пример показывает, как агент адаптируется под нестандартный стек
> вместо слепого копирования артефактов.

---

# Onboarding Report: Logistics.Worker

**Дата:** 2025-06-15  
**Режим:** standard  
**Агент:** skeptical-ai-bootstrap

---

## Сводка по принципам

| Слой | Принцип соблюдён | Текущее состояние | Решение |
|------|------------------|-------------------|---------|
| 1. Компилятор | 🟡 | Nullable enable, но нет TreatWarningsAsErrors | **Адаптировать**: Directory.Build.props |
| 2. Архитектура | 🔴 | Нет арх. тестов. Vertical Slice — стандартные правила NetArchTest (про слои) неприменимы | **Адаптировать**: NetArchTest с custom rules про границы фич |
| 3. Тесты | 🟡 | xUnit, 340 тестов, нет проверки "0 ran" | **Адаптировать**: verify-tests.sh для xUnit |
| 4. Code Review | 🔴 | Нет AGENTS.md. Dapper + MediatR — готовый скилл не подходит | **Создать скилл**: `code-review-dapper` |
| 5. E2E / MCP | 🔴 | Worker Service, нет HTTP. OpenAPI snapshot невозможен | **Создать скилл**: `e2e-worker` |
| 0. Инструкции | 🔴 | Нет AGENTS.md | **Внедрить**: `rules/AGENTS.md` |
| Внешний цикл | 🔴 | Нет аудитов. Dapper + SQL Server — готовый DBA не подходит | **Создать скилл**: `dba-audit-dapper` |

---

## Стек проекта

- **.NET:** 8.0 (`net8.0`)
- **Тип:** Worker Service (BackgroundService + RabbitMQ)
- **ORM/Данные:** Dapper 2.1 + SQL Server (нет EF Core)
- **Архитектура:** Vertical Slice (Features/Orders/, Features/Deliveries/)
- **Test framework:** xUnit (340 тестов, миграция на TUnit нецелесообразна)
- **CI:** GitHub Actions (простой `dotnet test`)
- **Messaging:** RabbitMQ + MassTransit

---

## Агенто-специфичная конфигурация

Проект использует **Claude Code**. Поэтому конфигурация guardrails будет в формате Claude:

```
.claude/
├── CLAUDE.md                      # Конституция (адаптирована из rules/AGENTS.md)
├── commands/
│   ├── code-review-dapper.md      # Code review для Dapper + MediatR
│   ├── architecture-audit.md        # NetArchTest с custom rules для VSlice
│   ├── task-compliance.md         # Scope check (адаптирован)
│   ├── e2e-worker.md              # E2E для Worker + RabbitMQ
│   ├── security-audit.md          # Адаптирован из skills/security-audit/
│   └── dba-audit-dapper.md        # DBA audit для Dapper
```

Если бы проект использовал **Kimi** — скиллы лежали бы в `.kimi/skills/`.
Если **Codex** — единый `.codex/instructions.md` с встроенными чеклистами.

---

## Почему не подходят готовые артефакты

### Архитектура: NetArchTest
Проект Vertical Slice. Границы — по фичам, а не по слоям.
NetArchTest проверяет `Domain` → `Application`, но здесь нет таких проектов.
Нужен сканер slice-зависимостей.

### Code Review: EF-специфичные правила
Готовый скилл проверяет `AsNoTracking`, `.Include()`, `[Authorize]`.
В проекте Dapper — эти правила бессмысленны. Нужны правила:
- Параметризация SQL (нет string interpolation)
- Async-методы Dapper (`QueryAsync`, не `Query`)
- Transaction scope в handlers

### E2E: OpenAPI Snapshot
Worker Service не имеет HTTP. E2E — это проверка обработки сообщений
из очереди и side-effects (записи в БД, логи, метрики).

### DBA Audit: EF-миграции
Готовый скилл смотрит EF миграции, `Include()`, `FindAsync()`.
В проекте Dapper — нужно проверять raw SQL, индексы, планы запросов.

---

## Бэклог внедрения

### Sprint 0 — Слой 0 + Компилятор (1 день)
- [ ] **Внедрить** `rules/AGENTS.md` → адаптировать под Worker + Dapper
- [ ] **Внедрить** `rules/CONVENTIONS.md`
- [ ] **Адаптировать** `Directory.Build.props`:
  - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
  - `<Nullable>enable</Nullable>`
- [ ] **Адаптировать** `.editorconfig` (severity = error для критичных правил)

### Sprint 1 — Архитектура (3 дня)
- [ ] **Адаптировать** `ArchitectureRules.cs` для Vertical Slice:
  - Проверка: Slice A не импортирует внутренности Slice B
  - Проверка: каждый Slice имеет Handler + Validator + Endpoint (или Worker consumer)
  - Проверка: Shared Kernel явно помечен // DECISION:
  - Реализация: NetArchTest custom rules + unit tests
  - Шаблон: `NEW-SKILL-TEMPLATE.md`
  - Положить в: `tests/ArchitectureTests/ArchitectureRules.VSlice.cs`

### Sprint 2 — Тесты (2 дня)
- [ ] **Адаптировать** verify-tests.sh для xUnit (не мигрировать на TUnit!)
  - Парсить выход `dotnet test --logger "console;verbosity=detailed"`
  - Проверить, что `Total tests: > 0`
- [ ] **Адаптировать** `tests/patterns/RatchetTest.cs`:
  - Добавить ratchet на кастомные атрибуты проекта (например, `[CriticalHandler]`) в дополнение к счётчику тестов
- [ ] **Создать** интеграционные тесты для RabbitMQ consumer:
  - Паттерн: In-memory queue + test host

### Sprint 3 — Code Review Agent (2 дня)
- [ ] **Создать скилл** `code-review-dapper`:
  - Правила: параметризованный SQL, async Dapper, transaction scope
  - Правила: MediatR handler не должен быть толстым (>50 строк)
  - Шаблон: `NEW-SKILL-TEMPLATE.md`
  - Положить в: `.claude/commands/code-review-dapper.md` (для Claude) или `.kimi/skills/code-review-dapper/` (для Kimi)
- [ ] **Адаптировать** `skills/task-compliance/` (подходит 1-к-1, заменить только stack-ссылки)

### Sprint 4 — E2E + CI (3 дня)
- [ ] **Создать скилл** `e2e-worker`:
  - Тест: публикуем сообщение в in-memory bus → проверяем обработку
  - Тест: проверяем side-effects (записи в БД, логи)
  - Тест: dead letter queue при ошибке
  - Шаблон: `NEW-SKILL-TEMPLATE.md`
- [ ] **Адаптировать** `ci/github-actions/safe-ci.yml`:
  - Заменить `dotnet run --project` на `dotnet test` (xUnit)
  - Добавить шаг: запуск Worker в test host для интеграционных тестов

### Backlog — Аудиты (раз в спринт)
- [ ] **Создать скилл** `dba-audit-dapper`:
  - Проверка: все SQL-запросы параметризованы
  - Проверка: нет `SELECT *` (explicit columns)
  - Проверка: индексы для WHERE-колонок (анализ через SQL Server DMVs)
  - Шаблон: `NEW-SKILL-TEMPLATE.md`
- [ ] **Адаптировать** `skills/security-audit/` (подходит 1-к-1)
- [ ] **Пропустить** `skills/i18n-audit/` (проект только русский, документировано)

---

## Новые скиллы для создания

| Скилл | Причина | Сложность |
|-------|---------|-----------|
| `ArchitectureRules.cs` (адаптированный) | Vertical Slice, нужны custom NetArchTest rules | Medium |
| `code-review-dapper` | Dapper + MediatR, готовый скилл про EF Core | Low |
| `e2e-worker` | Worker Service, нет HTTP/OpenAPI | Medium |
| `dba-audit-dapper` | Raw SQL, нужен аудит запросов и индексов | Medium |

**Всего новых скиллов:** 4  
**Всего адаптированных артефактов:** 5  
**Всего внедрённых 1-к-1:** 2 (`AGENTS.md`, `CONVENTIONS.md`)

---

## Проектирование новых скиллов (детали)

Агент спроектировал каждый новый скилл полностью — не просто имя, а роль, механизм, интеграция.

### `ArchitectureRules.cs` (адаптированный под VSlice)
- **Threat Model:** Агент добавляет cross-slice зависимость или ломает границу фичи
- **Роль:** Build Guard (срабатывает при сборке)
- **Механизм:** NetArchTest custom rules
- **Input:** Сборка проекта
- **Output:** Test result (pass/fail)
- **Trigger:** Каждый `dotnet build` → `dotnet run --project tests/ArchitectureTests/`
- **Gate:** BLOCKER
- **Правила:**
  - `Features/X/.*` не зависит от `Features/Y/.*` (кроме `Features/Shared/.*`)
  - Каждый slice имеет ровно один `*Handler`, один `*Validator`
  - Внутренние типы slice — `internal`, не `public`

### `code-review-dapper`
- **Threat Model:** SQL injection, blocking calls, N+1 через Dapper
- **Роль:** Reviewer (срабатывает на PR)
- **Механизм:** AI Agent (скилл)
- **Input:** Git diff PR
- **Output:** Комментарии в PR / отчёт
- **Trigger:** Каждый PR
- **Gate:** BLOCKER/CRITICAL/MAJOR/MINOR
- **Правила:**
  - Нет string interpolation в SQL (`$"SELECT ... {id}"`)
  - `QueryAsync`/`ExecuteAsync` вместо sync-методов
  - TransactionScope или `using var tran` в write-операциях
  - Метод >50 строк — разбить или обосновать // DECISION:

### `e2e-worker`
- **Threat Model:** Потеря сообщений, повторная обработка, отсутствие idempotency
- **Роль:** Integration Guard (срабатывает в CI)
- **Механизм:** Unit test + test host (In-memory RabbitMQ)
- **Input:** Сборка проекта
- **Output:** Test result (pass/fail)
- **Trigger:** Каждый PR + nightly
- **Gate:** BLOCKER
- **Правила:**
  - Публикуем сообщение → проверяем обработку за 5 секунд
  - Публикуем дубликат → проверяем idempotency (не 2 записи в БД)
  - Ломаем handler → проверяем retry + dead letter queue

### `dba-audit-dapper`
- **Threat Model:** Деградация perf из-за отсутствия индексов, N+1 в raw SQL
- **Роль:** DBA Auditor (срабатывает по триггеру)
- **Механизм:** AI Agent + SQL Server DMV query script
- **Input:** Git diff + схема БД
- **Output:** Отчёт с рекомендациями
- **Trigger:** Раз в спринт / при изменении `*.sql` или `*Repository*.cs`
- **Gate:** WARNING (не блокирует, но требует ack)
- **Правила:**
  - Все SQL с `WHERE` имеют покрывающий индекс (проверка через DMV)
  - Нет `SELECT *` (explicit column list)
  - Нет `TOP 100` без `ORDER BY`
  - Query execution time < 100ms на тестовых данных

---

## Карта экосистемы скиллов (генерируется)

```markdown
# Скиллы проекта Logistics.Worker

## Внутренний цикл
| Скилл | Роль | Механизм | Триггер | Gate | Статус |
|-------|------|----------|---------|------|--------|
| code-review-dapper | Reviewer | AI Agent | PR | BLOCKER | 🚧 WIP |
| task-compliance | Scope Guard | AI Agent | PR | BLOCKER | 📋 Backlog |
| architecture-audit (VSlice) | Build Guard | NetArchTest custom | Build | BLOCKER | 🚧 WIP |
| compiler-guard | Fast Feedback | MSBuild props | Build | BLOCKER | ✅ Active* |

* Sprint 0 активирует TreatWarningsAsErrors

## Внешний цикл
| Скилл | Роль | Механизм | Триггер | Gate | Статус |
|-------|------|----------|---------|------|--------|
| security-audit | Security Auditor | AI Agent | Weekly | WARNING | 📋 Backlog |
| dba-audit-dapper | DBA Auditor | AI Agent + Script | Sprint | WARNING | 🚧 WIP |
| performance-audit | Perf Auditor | AI Agent + NBomber | Release | BLOCKER | 📋 Backlog |

## Проектно-специфичные
| Скилл | Роль | Механизм | Триггер | Gate | Статус |
|-------|------|----------|---------|------|--------|
| e2e-worker | Integration Guard | Unit Tests | PR + Nightly | BLOCKER | 🚧 WIP |
```

---

## Не применимо (документировано)

- **OpenAPI Snapshot** — проект Worker Service, нет HTTP endpoints
- **i18n Audit** — проект только для рынка РФ, русский язык
- **Стандартные NetArchTest rules** — правила про слои (Domain→Application) не применимы к VSlice. Заменены на custom rules про границы фич
- **TUnit миграция** — 340 тестов на xUnit, миграция дороже выгоды

---

## Риски

1. **4 новых скилла** — это работа на 2-3 дня только на описание правил. Но без них guardrails будут бесполезны.
2. **xUnit остаётся** — не мигрируем на TUnit, потому что 340 тестов + инфраструктура. verify-tests.sh решает проблему "0 ran".
3. **Vertical Slice + Dapper** — нет готовых паттернов в `dotnet-skeptical-ai`. Но принципы те же: авто-проверка, ratchet, code review.

---

## Рекомендация

Начать со **Sprint 0** (слой 0) + адаптации `ArchitectureRules.cs` под VSlice.
Это даёт 60% эффекта: агенты перестанут ломать slice-границы и nullable.

Остальные скиллы можно создавать по мере необходимости — важно,
чтобы **принципы** работали, а не конкретные инструменты.
