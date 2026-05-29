# DBA Audit Checklist

## EF Core Queries
- [ ] Read-path: `.Select()` + `.AsNoTracking()`
- [ ] Write-path: NO `.AsNoTracking()`
- [ ] No `.Include()` in read-path
- [ ] No `.FindAsync()` in read-path
- [ ] Batch queries for nested collections

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

## Query Analysis
- [ ] New queries checked with EXPLAIN ANALYZE
- [ ] No Seq Scan on large tables without фильтра
- [ ] No N+1 detected in integration tests logs
