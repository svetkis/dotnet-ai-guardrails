# Performance Audit — Checklist

## Hot Paths
- [ ] Most frequent queries identified
- [ ] SQL query count per request calculated
- [ ] Indexes on WHERE conditions checked
- [ ] Projections `.Select()` used in read-path (projections do not require `.AsNoTracking()`)

## EF Core (if used)
- [ ] Read-path without projection → `.AsNoTracking()`. Projections `.Select()` — exception.
- [ ] Write-path → no `.AsNoTracking()`. Exception: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).

## N+1
- [ ] Loops with DB queries inside checked
- [ ] Background jobs checked

## API
- [ ] All endpoints with pagination
- [ ] Summary DTO instead of full entities

## Cache
- [ ] Keys centralized (CacheKeys.cs)
- [ ] Size specified for every entry
- [ ] No type conflicts under one key
- [ ] Invalidation covers all write-paths

## Jobs
- [ ] Intervals are optimal
- [ ] Projections + ExecuteUpdateAsync used

## Metrics
- [ ] P50 / P99 / Max latency measured
- [ ] Read + Write mix tested
- [ ] Concurrent load checked
