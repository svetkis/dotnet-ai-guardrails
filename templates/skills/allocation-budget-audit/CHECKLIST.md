# Allocation Budget Audit — Checklist

## Before Starting
- [ ] Critical project paths are known (latency-sensitive)
- [ ] Hot path marker or equivalent exists
- [ ] Stable measurement environment is defined (OS, runtime, GC mode)

## Hot Path Inventory
- [ ] All hot path methods are found (via reflection, static analysis, registry)
- [ ] Every method has an allocation test or equivalent guardrail
- [ ] Hot path marker is not used for rare operations

## Allocation Budget
- [ ] Baseline is recorded for every hot path
- [ ] Regression threshold is defined (baseline + N% or project-specific)
- [ ] Tests use warmup + multiple iterations
- [ ] CI runs tests on relevant hardware

## Regressions
- [ ] Methods over budget are reviewed
- [ ] Regression reason is documented
- [ ] Fix is confirmed by repeated measurement

## Compile-time Guardrail
- [ ] If the project has a hot path analyzer — it catches forbidden patterns
- [ ] Analyzer is covered by its own unit tests

## Report Format
- [ ] Summary by method with baseline/current values
- [ ] Regression list with causes
- [ ] Missing budget tests list
