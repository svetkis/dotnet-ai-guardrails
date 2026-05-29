# Code Review — Checklist

## Before Start
- [ ] Diff obtained (`git diff`)
- [ ] Task context known (backlog item / spec)

## Security
- [ ] XSS: query params, returnUrl stripped
- [ ] ID leak: internal ClientId/OwnerId not returned in API
- [ ] JWT not logged
- [ ] Rate limiting on public endpoints
- [ ] Constant-time comparison for hashes

## EF Core
- [ ] Read-path without projection: `.AsNoTracking()` present. Projections `.Select()` to DTO — not required.
- [ ] Write-path: no `.AsNoTracking()`. Exception: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).
- [ ] No `.Include()` in QueryService
- [ ] No `.FindAsync()` in read-path

## Architecture
- [ ] If project uses Clean Architecture (has Domain / Infrastructure projects) — Domain does not depend on Infrastructure
- [ ] API returns DTO/records, not Entities
- [ ] CancellationToken accepted by all async methods

## Tests
- [ ] Every `fix:` commit has `BUG*Tests.cs` or modification of existing
- [ ] New functionality covered by tests
- [ ] No `Assert.That(true)` or empty placeholder tests

## Code Quality
- [ ] No `async void`
- [ ] No empty `catch { }`
- [ ] DateTime in UTC (`DateTime.UtcNow`, `DateTimeKind.Utc`)
- [ ] PostgreSQL columns: `snake_case`

## Report Format

```markdown
## Code Review — {date}

### BLOCKER
- [ ] {description} → {file:line}

### CRITICAL
- [ ] {description} → {file:line}

### MAJOR
- [ ] {description} → {file:line}

### Verdict
- [ ] APPROVED
- [ ] APPROVED_WITH_NITS
- [ ] CHANGES_REQUESTED
```
