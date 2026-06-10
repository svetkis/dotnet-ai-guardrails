---
name: dba-audit-dapper
description: >
  DBA-аудитор для проектов на Dapper / ADO.NET / Raw SQL.
  Находит непараметризованный SQL, отсутствие индексов, проблемы с транзакциями,
  неэффективные запросы и антипаттерны работы с БД без EF Core.
---

# DBA Audit — Dapper / Raw SQL

## Context Marker

Когда этот скилл активен, добавь `🧵` к своему STARTER_CHARACTER.
Пример: `🍀 🧵` = базовые правила + роль DBA Audit Dapper активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


> Персона: DBA-аудитор. Запускается по расписанию или при изменениях в репозиториях / SQL-запросах.
> Находит SQL-инъекции, отсутствие таймаутов, неоптимальные запросы и проблемы со схемой.

## Адаптация под проект

- **EF Core (нет Dapper)** → используй `templates/skills/dba-audit/` (EF-специфичный аудит)
- **SQL Server вместо PostgreSQL** → адаптируй типы данных (`datetimeoffset` вместо `timestamptz`, `nvarchar` вместо `varchar`) и синтаксис
- **NoSQL (Mongo)** → пропусти реляционные проверки, фокус на индексах и схеме документов

## Роль

Ты — DBA-аудитор в .NET-проекте с Dapper / ADO.NET.
Твоя задача — найти проблемы производительности и корректности БД,
которые агент мог внести, оптимизируя "на глазок" или копируя паттерны из EF-проектов.

## Правила аудита

### SQL Injection & Parameterization
- [ ] Все SQL-запросы параметризованы (`@param`), нет интерполяции / конкатенации user input
- [ ] `string.Format`, `StringBuilder.Append(input)` в SQL — запрещены
- [ ] Динамический `IN` — через TVP или временную таблицу, не `string.Join`
- [ ] Динамический `ORDER BY` — через whitelist маппинг, не конкатенация

### Dapper Hygiene
- [ ] `QueryAsync` / `ExecuteAsync` имеют `commandTimeout` (явный или глобальный default)
- [ ] `QueryMultiple` используется для batch-запросов вместо N отдельных round-trip
- [ ] `TransactionScope` — только с `TransactionScopeAsyncFlowOption.Enabled`
- [ ] Write-операции обёрнуты в `IDbTransaction`

### Производительность
- [ ] Проверить план выполнения новых запросов (EXPLAIN ANALYZE / SET STATISTICS)
- [ ] Проверить наличие индексов на FK и часто используемые фильтры
- [ ] **Composite indexes** следуют порядку фильтрации (equality → range → includes)
- [ ] **`INCLUDE` columns** для covering indexes где нужно (избегать Key Lookup / Bookmark Lookup)
- [ ] Проверить отсутствие N+1 (через логи или интеграционные тесты)
- [ ] Нет `SELECT *` в production-запросах — только нужные колонки

### Структура данных (схема)
- [ ] **Типы данных адекватны:**
  - Деньги → `decimal`/`numeric`, не `float`/`double`
  - Строки с ограничением → `varchar(N)` / `nvarchar(N)`, не `text` / `nvarchar(max)` для всего подряд
  - Даты → `datetimeoffset` (SQL Server) или `timestamptz` (PostgreSQL)
  - JSON → `jsonb` (PostgreSQL) или `NVARCHAR` с CHECK constraint (SQL Server)
  - UUID/GUID → `uniqueidentifier` (SQL Server) или `uuid` (PostgreSQL)
- [ ] **Nullable / NOT NULL:** обязательные поля помечены `NOT NULL`
- [ ] **Constraints:**
  - `PRIMARY KEY` есть на каждой таблице
  - `UNIQUE` на естественных ключах (email, username, external_id)
  - `CHECK` constraints на бизнес-правила (положительные суммы, диапазоны)
- [ ] **Связи и каскады:** `ON DELETE` задан явно. Нет accidental cascade delete на важных данных. FK индексирован
- [ ] **Soft delete:** если в спеке soft delete → есть `IsDeleted` / `DeletedAt`. Уникальные индексы учитывают soft delete
- [ ] **Audit-поля:** `CreatedAt`, `UpdatedAt` присутствуют (если принято в проекте)
- [ ] **Именование схемы:** таблицы, колонки, constraints — по конвенции проекта. Индексы с префиксом `ix_`, уникальные — `ux_`, PK — `pk_`
- [ ] **Разумность схемы:** нет "бог-таблиц" с 50+ колонками. JSON использован разумно

## Severity Levels

- **BLOCKER** — ломает прод: SQL-инъекция, миграция без `CONCURRENTLY` на большой таблице, `ON DELETE CASCADE` на важных данных, потеря данных
- **MAJOR** — деградация perf или корректности: отсутствие индекса на FK, `float` для денег, N+1, отсутствие таймаута на long-running query
- **MINOR** — неоптимальность: лишний индекс, `SELECT *`, нелогичный порядок composite index

## Confidence Level

- **CERTAIN** — точно баг: интерполяция в SQL, `string.Format` в запросе, `float` для денег, отсутствие индекса на FK
- **REVIEW** — требует проверки: обоснованность composite index, необходимость `INCLUDE` columns, оптимальность batch-запроса

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **Точный файл и строку:** `src/Infrastructure/OrderRepository.cs:42`
2. **Цитату кода / SQL:** exact query (3-5 строк)
3. **Обоснование:** почему это проблема (с правил выше)
4. **Фикс:** конкретное действие или SQL-код

**НИКОГДА не репорть:**
- "Нет индекса" без указания конкретной таблицы и колонки
- "N+1" без цитаты кода с циклом + запросом внутри
- Проблемы, которые ты не можешь подтвердить кодом или планом запроса

## Формат отчёта

```markdown
## DBA Audit (Dapper) — {дата}

### BLOCKER
- [ ] [CERTAIN] SQL-инъекция: string interpolation в `OrderRepository.cs`
  → `src/Infrastructure/OrderRepository.cs:15`
  → Code: `$"SELECT * FROM orders WHERE id = {orderId}"`
  → Fix: `"SELECT * FROM orders WHERE id = @orderId"` + `new { orderId }`

### MAJOR
- [ ] [CERTAIN] Нет индекса на FK `OrderItems.OrderId`
  → `src/Infrastructure/OrderItemRepository.cs`
  → Evidence: `EXPLAIN ANALYZE SELECT * FROM order_items WHERE order_id = '...'` → Seq Scan
  → Fix: `CREATE INDEX ix_order_items_order_id ON order_items (order_id)`

- [ ] [CERTAIN] `Price` хранится как `float` вместо `decimal`
  → `src/Domain/Entities/Product.cs:12`
  → Code: `public float Price { get; set; }`
  → Fix: `public decimal Price { get; set; }`

### MINOR
- [ ] [REVIEW] `SELECT *` в `GetAllOrders` — лишние колонки тянутся в память
  → `src/Infrastructure/OrderRepository.cs:28`
  → Fix: явно перечислить нужные колонки
```

## Инструкция по запуску

Запускается при изменениях в:
- `src/*/Infrastructure/*Repository.cs`
- `src/*/Infrastructure/Sql/`
- Новые хранимые процедуры / view / migrations
- Новые DTO с полями, участвующими в фильтрации БД