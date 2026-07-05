# DBA Audit Checklist — Dapper / Raw SQL

## SQL Injection & Parameterization
- [ ] All SQL queries are parameterized (`@param`)
- [ ] No C# string interpolation (`$"..."`) in SQL
- [ ] No concatenation of user input with SQL
- [ ] Dynamic `IN` — via TVP or temp table
- [ ] Dynamic `ORDER BY` — via whitelist, not concatenation

## Dapper Hygiene
- [ ] `QueryAsync` / `ExecuteAsync` have `commandTimeout`
- [ ] `QueryMultiple` for batch queries instead of N round-trips
- [ ] `TransactionScope` with `TransactionScopeAsyncFlowOption.Enabled`
- [ ] Write operations inside `IDbTransaction`

## Indexes
- [ ] FK columns have indexes
- [ ] Frequently filtered columns have indexes
- [ ] Composite indexes follow filter order
- [ ] `INCLUDE` columns for covering indexes

## Query Performance
- [ ] New queries checked with execution plan
- [ ] No `SELECT *` in production queries
- [ ] No N+1 detected in logs / tests

## Schema
- [ ] Data types are adequate: decimal for money, datetimeoffset/timestamptz for dates, uuid/uniqueidentifier for GUIDs
- [ ] Strings have length limits: varchar(N), not text/max without reason
- [ ] Required fields are marked NOT NULL
- [ ] PRIMARY KEY on every table
- [ ] UNIQUE constraints on natural keys
- [ ] CHECK constraints on business rules
- [ ] ON DELETE is explicit, no accidental cascade
- [ ] FK columns are indexed
- [ ] Soft delete is handled: IsDeleted + partial unique indexes
- [ ] Audit fields: CreatedAt / UpdatedAt
