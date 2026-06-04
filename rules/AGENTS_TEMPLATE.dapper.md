# AGENTS.md — Dapper / Raw SQL Add-on

> Дополнение к `AGENTS_TEMPLATE.md` для проектов с **Dapper, ADO.NET или raw SQL**.
> Скопируйте содержимое этого файла в конец вашего `AGENTS.md`.
> Архитектурные guardrails: [`tests/patterns/DapperGuardRules.cs`](../tests/patterns/DapperGuardRules.cs)

## Semantic Anchors (Dapper)

| Term | Meaning |
|------|---------|
| **Read-path** | Query path: `QueryAsync` / `Query` projections, no tracking concerns |
| **Write-path** | Command path: `ExecuteAsync` / `Execute` inside explicit transaction |

## Raw SQL

- ❌ C# string interpolation (`$"..."`) в SQL-строке — **FORBIDDEN**
- ❌ Конкатенация (`+`) user input с SQL — **FORBIDDEN**
- ✅ Параметризация (`@param`) — **MANDATORY**
- ✅ `IN` с динамическим списком — через Table-Valued Parameter (TVP) или временную таблицу, не `string.Join`

## Dapper Conventions

- ✅ Каждый вызов `QueryAsync` / `ExecuteAsync` должен иметь `commandTimeout` (явный или глобальный default)
- ✅ Write-операции обёрнуты в `IDbTransaction` — нет standalone `ExecuteAsync` для изменений
- ✅ `TransactionScope` только с `TransactionScopeAsyncFlowOption.Enabled`
- ✅ `QueryMultiple` для batch-запросов вместо N отдельных вызовов

## Raw SQL Hygiene in EF Projects

- ✅ Даже в EF-проектах `FromSqlRaw` / `ExecuteSqlRaw` требуют параметризации
- ❌ `FromSqlRaw` с `$"..."` — **FORBIDDEN** (используй `FromSqlInterpolated`)

## Hard Prohibitions (Dapper)

- ❌ `string.Format`, `StringBuilder.Append(userInput)` в SQL
- ❌ Динамический `ORDER BY` через конкатенацию без whitelist
