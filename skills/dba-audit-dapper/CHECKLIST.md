# DBA Audit Checklist — Dapper / Raw SQL

## SQL Injection & Parameterization
- [ ] Все SQL-запросы параметризованы (`@param`)
- [ ] Нет C# string interpolation (`$"..."`) в SQL
- [ ] Нет конкатенации user input с SQL
- [ ] Динамический `IN` — через TVP или временную таблицу
- [ ] Динамический `ORDER BY` — через whitelist, не конкатенация

## Dapper Hygiene
- [ ] `QueryAsync` / `ExecuteAsync` имеют `commandTimeout`
- [ ] `QueryMultiple` для batch-запросов вместо N round-trip
- [ ] `TransactionScope` с `TransactionScopeAsyncFlowOption.Enabled`
- [ ] Write-операции в `IDbTransaction`

## Indexes
- [ ] FK columns have indexes
- [ ] Frequently filtered columns have indexes
- [ ] Composite indexes follow порядок фильтрации
- [ ] `INCLUDE` columns для covering indexes

## Query Performance
- [ ] New queries checked with execution plan
- [ ] No `SELECT *` in production queries
- [ ] No N+1 detected in logs / tests

## Schema
- [ ] Типы данных адекватны: decimal для денег, datetimeoffset/timestamptz для дат, uuid/uniqueidentifier для GUID
- [ ] Строки с ограничением длины: varchar(N), не text/max без причины
- [ ] Обязательные поля помечены NOT NULL
- [ ] PRIMARY KEY на каждой таблице
- [ ] UNIQUE constraints на естественных ключах
- [ ] CHECK constraints на бизнес-правилах
- [ ] ON DELETE задан явно, нет accidental cascade
- [ ] FK-колонки проиндексированы
- [ ] Soft delete учтён: IsDeleted + partial unique indexes
- [ ] Audit-поля: CreatedAt / UpdatedAt
