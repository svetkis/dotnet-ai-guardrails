# Code Review — Checklist

## Before Start
- [ ] Staged diff obtained (`git diff --cached`)
- [ ] Task context known (backlog item / spec)
- [ ] Skill activated automatically before `git commit` or explicitly via `/skill:code-review`

## Pre-commit / Trigger
- [ ] Staged changes include .NET backend files (*.cs, *.csproj, *.sln, *.props, *.targets)
- [ ] Frontend-only changes are skipped (use separate frontend-code-review skill)
- [ ] When staged diff is empty, agent reports nothing and does not block commit
- [ ] Agent does NOT run `git commit` itself

## Security
- [ ] XSS: query params, returnUrl stripped
- [ ] ID leak: internal ClientId/OwnerId not returned in API
- [ ] JWT not logged
- [ ] Rate limiting on public endpoints
- [ ] Constant-time comparison for hashes

## EF Core
- [ ] Read-path without projection: `.AsNoTracking()` present. Projections `.Select()` to DTO — not required.
- [ ] Write-path: no `.AsNoTracking()`. Exception: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).
- [ ] `.Include()` in QueryService justified; no over-fetched chains without projection
- [ ] `.FindAsync()` in read-path reasonable for PK reads; flag for lists/filters

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

## Business Logic Duplication (Semantic)
> Automated tests catch only literal copying. This block is for humans.
- [ ] New validation/status calculation — does similar logic exist in other services?
- [ ] Can the rule be extracted into a Domain Service / Value Object / `BR-###`?
- [ ] No divergence: in one place `>= 100`, in another `> 100`

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
