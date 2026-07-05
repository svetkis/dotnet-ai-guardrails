# DBA Audit Checklist

## EF Core Queries
- [ ] Read-path without projection: `.AsNoTracking()` is present (optional, for read-only scenarios)
- [ ] Write-path: NO `.AsNoTracking()` (exception: raw SQL, bulk API)
- [ ] `.Include()` is justified: no redundant chains of 3+ navigations without explicit comment
- [ ] `.FindAsync()` is used reasonably: acceptable for reading by PK, flag if used for lists/filters
- [ ] Batch queries for nested collections (if applicable)

## Indexes
- [ ] FK columns have indexes
- [ ] Frequently filtered columns have indexes
- [ ] Composite indexes follow filter order
- [ ] `INCLUDE` columns for covering indexes where needed

## Migrations Safety
- [ ] Raw SQL — PostgreSQL compatible
- [ ] Column rename — Add + Drop (not Rename)
- [ ] Index creation on large tables — `CONCURRENTLY`
- [ ] No data loss in migration

## Data Structure (Schema)
- [ ] Data types are adequate: decimal for money, timestamptz for dates, uuid for GUID, jsonb for JSON
- [ ] Strings with length limit: varchar(N), not text/varchar(max) without reason
- [ ] Required fields are marked NOT NULL (IsRequired in EF)
- [ ] PRIMARY KEY on every table
- [ ] UNIQUE constraints on natural keys
- [ ] CHECK constraints on business rules (positive sums, etc.)
- [ ] ON DELETE is set explicitly on FK, no accidental cascade delete
- [ ] Indexes on FK columns
- [ ] Soft delete accounted: IsDeleted / DeletedAt + partial unique indexes
- [ ] Audit fields: CreatedAt / UpdatedAt (and CreatedBy / UpdatedBy if needed)
- [ ] Naming: snake_case for tables/columns, prefixes ix_/ux_/pk_ for indexes
- [ ] No "god tables" with 50+ columns, JSONB used reasonably
- [ ] Partitioning considered for tables >10M records or time-series

## Query Analysis
- [ ] New queries checked with EXPLAIN ANALYZE
- [ ] No Seq Scan on large tables without filter
- [ ] No N+1 detected in integration tests logs
