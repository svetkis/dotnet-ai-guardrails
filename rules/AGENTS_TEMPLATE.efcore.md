# AGENTS.md — EF Core Add-on

> Дополнение к `AGENTS_TEMPLATE.md` для проектов с **Entity Framework Core**.
> Скопируйте содержимое этого файла в конец вашего `AGENTS.md`.
> Архитектурные guardrails: [`tests/patterns/EfCoreGuardRules.cs`](../tests/patterns/EfCoreGuardRules.cs)

## Semantic Anchors (EF Core)

| Term | Meaning |
|------|---------|
| **Read-path** | Query path: `.Select()` + `.AsNoTracking()` mandatory, change tracking forbidden |
| **Write-path** | Command path: full entity + change tracking required, `.AsNoTracking()` forbidden |

## Entity Framework

### Read-path (no exceptions)
- ❌ EF queries without `.Select()` — **FORBIDDEN**
- ❌ `.Include()` in read-path — **FORBIDDEN**
- ❌ `.FindAsync()` in read-path — **FORBIDDEN**
- ✅ `.Select()` + `.AsNoTracking()` — **MANDATORY**
- ✅ Nested collections in `.Select()` — extract to separate batch query

### Write-path
- ✅ Full entity loading allowed (change tracking required)
- ✅ `.FindAsync()` allowed only in write/command scenarios
- ❌ `.AsNoTracking()` in write-path — **FORBIDDEN**

### Raw SQL in EF
- ✅ `FromSqlInterpolated` — **MANDATORY** для параметризованного SQL
- ❌ `FromSqlRaw` с `$"..."` интерполяцией — **FORBIDDEN**
- ✅ `ExecuteSqlRaw` только со статическими строками или параметрами

## Hard Prohibitions (EF Core)

- ❌ Global `QueryTrackingBehavior.NoTracking` — only explicit `.AsNoTracking()` in read methods
