# Backlog Hygiene Checklist

## Stale
- [ ] No tasks older than 90 days without updates
- [ ] Merged specs are closed in backlog
- [ ] CHANGELOG items are linked to closed tasks

## Orphaned
- [ ] Every spec has implementation, is marked `deferred`, or removed
- [ ] Every `BUG###_` test has a task or CHANGELOG entry
- [ ] No specs for deleted modules

## Duplicates
- [ ] No duplicate tasks in `.backlog/`
- [ ] No duplicate ACs in different specs

## Priority
- [ ] `Must` actually blocks upcoming release
- [ ] `Won't` does not turn into hidden tech debt
- [ ] `Could` is not silently implemented in production

## Traceability
- [ ] Every open task has a spec or AC
- [ ] Every task references code/test/PR

## Actionability
- [ ] Title contains verb + object (not just a noun)
- [ ] Has Definition of Done (1-3 items) or AC
- [ ] No tasks with title < 5 words and no AC

## Source Tagging
- [ ] Every task tagged `[human]` or `[agent]`
- [ ] `[agent]` without human approval > 14 days marked for archive
- [ ] `[agent]` with human approval have explicit source

## Test Debt
- [ ] Every new `[HotPath]` has a task for perf-test
- [ ] Every new public endpoint has a task for snapshot-test
- [ ] Every new `[SensitiveData]` has a task for PiiGuardTest
