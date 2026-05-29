# DBA Audit Checklist

## EF Core Queries
- [ ] Read-path без проекции: `.AsNoTracking()` присутствует (опционально, для read-only сценариев)
- [ ] Write-path: NO `.AsNoTracking()` (исключение: raw SQL, bulk API)
- [ ] `.Include()` обоснован: нет избыточных цепочек из 3+ навигаций без явного комментария
- [ ] `.FindAsync()` используется разумно: допустим для чтения по PK, флаг если используется для списков/фильтров
- [ ] Batch queries for nested collections (если применимо)

## Indexes
- [ ] FK columns have indexes
- [ ] Frequently filtered columns have indexes
- [ ] Composite indexes follow порядок фильтрации
- [ ] `INCLUDE` columns для covering indexes где нужно

## Migrations Safety
- [ ] Raw SQL — PostgreSQL compatible
- [ ] Column rename — Add + Drop (not Rename)
- [ ] Index creation on large tables — `CONCURRENTLY`
- [ ] No data loss in migration

## Структура данных (схема)
- [ ] Типы данных адекватны: decimal для денег, timestamptz для дат, uuid для GUID, jsonb для JSON
- [ ] Строки с ограничением длины: varchar(N), не text/varchar(max) без причины
- [ ] Обязательные поля помечены NOT NULL (IsRequired в EF)
- [ ] PRIMARY KEY на каждой таблице
- [ ] UNIQUE constraints на естественных ключах
- [ ] CHECK constraints на бизнес-правилах (положительные суммы и т.д.)
- [ ] ON DELETE задан явно на FK, нет accidental cascade delete
- [ ] Индексы на FK-колонках
- [ ] Soft delete учтён: IsDeleted / DeletedAt + partial unique indexes
- [ ] Audit-поля: CreatedAt / UpdatedAt (и CreatedBy / UpdatedBy если нужно)
- [ ] Именование: snake_case для таблиц/колонок, префиксы ix_/ux_/pk_ для индексов
- [ ] Нет "бог-таблиц" с 50+ колонками, JSONB использован разумно
- [ ] Partitioning рассмотрен для таблиц >10M записей или time-series

## Query Analysis
- [ ] New queries checked with EXPLAIN ANALYZE
- [ ] No Seq Scan on large tables without фильтра
- [ ] No N+1 detected in integration tests logs
