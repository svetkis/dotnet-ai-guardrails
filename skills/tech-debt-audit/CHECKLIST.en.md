# Tech Debt Audit — Checklist

## Before Start
- [ ] Access to codebase obtained (`git log --since="3 months ago"` for context)
- [ ] Project stack known (Clean Architecture / MVP / Minimal API)

## Semantic Duplication
- [ ] Validation/calculation duplicated in 2+ places differently
- [ ] Threshold divergence (`>=` vs `>`, `==` vs `Equals`)
- [ ] Can be extracted to Domain Service / `BR-###`

## Stale Abstractions
- [ ] Interface with one implementation (except Ports)
- [ ] Abstract class without inheritors
- [ ] Method never called (except in tests)

## Dead Code
- [ ] `[Obsolete]` without removal date
- [ ] Constant feature flags
- [ ] Commented-out blocks > 3 lines

## Architectural Drift
- [ ] New Domain → Infrastructure / Api dependencies
- [ ] Circular dependencies between projects
- [ ] API directly uses Infrastructure

## Test Debt
- [ ] Critical paths without tests
- [ ] `BUG###_` tests no longer reproduce bug
- [ ] Characterization tests stale

## Documentation Drift
- [ ] `AGENTS.md` contradicts code
- [ ] Numbered Decisions reference deleted code
- [ ] `TODO` / `FIXME` without backlog item
