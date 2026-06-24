# Allocation Budget Audit — Checklist

## Before Starting
- [ ] Critical project paths are known (latency-sensitive)
- [ ] `[HotPath]` marker or equivalent exists
- [ ] Stable measurement environment is defined (OS, runtime, GC mode)

## Hot Path Inventory
- [ ] All `[HotPath]` methods are found via reflection
- [ ] Every method has a `{MethodName}_AllocationBudget` test
- [ ] `[HotPath]` is not used for rare operations

## Allocation Budget
- [ ] Baseline is recorded for every hot path
- [ ] Threshold is defined (baseline + 10% or project-specific)
- [ ] Tests use warmup + multiple iterations
- [ ] CI runs tests on relevant hardware

## Regressions
- [ ] Methods over budget are reviewed
- [ ] Regression reason is documented
- [ ] Fix is confirmed by repeated measurement

## Roslyn-first
- [ ] `HotPathAnalyzer` catches new / async / boxing in `[HotPath]`
- [ ] Analyzer is covered by unit tests

## Report Format
- [ ] Summary by method with baseline/current values
- [ ] Regression list with causes
- [ ] Missing budget tests list
