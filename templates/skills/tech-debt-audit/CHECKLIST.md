# Tech Debt Audit — Checklist

## Before Start
- [ ] Access to codebase obtained (`git log --since="3 months ago"` for context)
- [ ] Project stack and architecture known

## Semantic Duplication
- [ ] Validation/calculation duplicated in 2+ places differently
- [ ] Threshold divergence (`>=` vs `>`, `==` vs `Equals`)
- [ ] Can be extracted to Domain Service / numbered business-rule ID

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
- [ ] Bug-regression tests no longer reproduce bug
- [ ] Characterization tests stale

## Documentation Drift
- [ ] Project documentation contradicts code
- [ ] Decision Guards reference deleted code
- [ ] `TODO` / `FIXME` without backlog item
