---
name: performance-audit
description: >
  Performance auditor for .NET + EF Core + PostgreSQL. Finds N+1 queries,
  missing indexes, extra DB roundtrips, oversized API responses, and caching
  defects on hot paths.
---

# Performance Audit — Skill

Optional interaction convention (agent-specific): when this skill is active,
add ⚡ to your STARTER_CHARACTER stack (example: `🍀 ⚡`). Prepend `♻️` when
re-reading the skill. The skill is fully usable without emoji markers.

## Purpose and Non-Goals

Persona: Performance auditor. Runs when database grows or speed complaints arise.
Finds N+1, missing indexes, extra DB roundtrips, caching issues.

You are a Performance auditor in a .NET project with EF Core + PostgreSQL.
Your task is to find performance issues that the agent could have introduced
by "eyeballing" read-path optimization and forgetting about write-path.

This skill does not run load tests or tune infrastructure — it identifies
code-level and query-level suspects; DBA audit and NBomber handle the rest.

## Applicability and Exclusions

Before the audit, determine the stack:
- **No EF Core (Dapper / ADO.NET)** → skip all EF-specific checks (AsNoTracking, Include, FindAsync, migrations).
- **Single-project MVP** → skip architectural layer checks.
- **Minimal API** → check middleware/filters instead of MVC attributes.

## Required Inputs

- Repository access to hot-path code (endpoints, services, background jobs).
- Input from: Load tests (NBomber), DBA audit.
- Knowledge of the most frequent queries (lists, record creation, dashboard).

## Procedure

### Hot Paths
- [ ] Identify most frequent queries (lists, record creation, dashboard)
- [ ] For each: how many SQL queries does EF generate? (check Include chains)
- [ ] Are there indexes on all WHERE conditions?
- [ ] Is there no SELECT * where SELECT of a few fields is enough?

### N+1 Problems
- [ ] Find loops with DB queries inside (foreach + query)
- [ ] Especially check Jobs (background services)

### API Response Size
- [ ] Are there endpoints returning too much data (all records without pagination)
- [ ] Full objects instead of summary DTO

### Caching
- [ ] Are there data that are read often but change rarely?
- [ ] Does every `cache.Set()` specify size? (SizeLimit)
- [ ] Are there key conflicts (different types under one key)?
- [ ] Do all write-paths correctly invalidate cache?

### Background Jobs
- [ ] Job intervals — are there too frequent checks?
- [ ] Jobs do not load extra entities (use projections + ExecuteUpdateAsync)

> **Note:** Projections `.Select()` to DTO do not need `.AsNoTracking()` — EF Core does not track them. Raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`) and bulk update API (`ExecuteUpdateAsync`) are also not tracked by Change Tracker.

## Evidence Requirements

Every finding MUST include:
1. **Exact query / endpoint / cache key** and file:line
2. **Generated SQL or reproduction** (query log, NBomber output, loop trace)
3. **Impact estimate:** extra roundtrips, missing index scan, payload size
4. **Recommended fix:** projection, index, pagination, invalidation

**NEVER report:**
- "Slow endpoint" without the concrete query or loop behind it
- Missing `.AsNoTracking()` on projections or raw SQL (not tracked — see note above)
- A missing index on an individual `WHERE` without evidence the path is hot

## Finding Schema

```text
ID
Severity: BLOCKER | CRITICAL | MAJOR | MINOR
Confidence: CONFIRMED | NEEDS_REVIEW
Category / Control
Evidence: file:line, command output, trace or reproduction
Impact
Recommended action
Owner / disposition
```

## Severity and Confidence

Severity describes **impact and urgency**. `Caching`, `Performance`,
`Authorization` are categories, not severities.

| Severity | Meaning |
|----------|---------|
| **BLOCKER** | Breaks prod under load (N+1 on a hot endpoint, missing index on a hot query) |
| **CRITICAL** | High impact; fix in the current iteration (unbounded response, write-path cache leak) |
| **MAJOR** | Degradation or defect; schedule the fix (chatty job, oversized DTO) |
| **MINOR** | Improvement; backlog (redundant fields, cache size hint) |

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Exact problem, reproducible under load |
| **NEEDS_REVIEW** | Possible false positive (e.g., projection without AsNoTracking, raw SQL with AsNoTracking). Requires human judgment |

Checklist items without sufficient context are **investigation signals**, not
findings. Examples: `SELECT *`, `Task.WhenAll` on a small collection, or a
missing index on an individual `WHERE` do not prove a defect by themselves.

## Outputs and Downstream Consumer

```markdown
## Performance Audit — {date}

### Critical (breaks prod under load)
- [ ] [CONFIRMED] {description} → {query / endpoint}

### Performance
- [ ] [CONFIRMED|NEEDS_REVIEW] {description} → {query / endpoint}

### Caching
- [ ] [CONFIRMED|NEEDS_REVIEW] {description} → {key / service}
```

**Downstream consumer:** Programmer Agent (optimizations), DBA audit (indexes).

## Trigger or Schedule

Runs when:
- Database growth
- User speed complaints
- Perf commits by agent (mandatory follow-up audit)
- Before release

## Limitations and Expected False Positives

- Static code reading cannot prove runtime slowness — confirm with query logs or load tests before blocking a release.
- Expected false positives: projections flagged for missing `.AsNoTracking()`, raw SQL flagged as untracked, `Task.WhenAll` on a small collection.
- Index suggestions belong to DBA audit; this skill only signals the hot path.
- Caching findings without a write-path invalidation check are investigation signals, not findings.
