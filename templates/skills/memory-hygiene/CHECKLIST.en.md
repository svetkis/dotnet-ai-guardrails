# Memory Hygiene Checklist

## Semantic Duplicates
- [ ] Intent-duplicates found (not just verbatim)
- [ ] Duplicates of AGENTS.md in Auto Memory marked for deletion
- [ ] Rephrasings of the same rule grouped

## Hierarchical Drift
- [ ] Every architectural note checked against nearest AGENTS.md
- [ ] Practical details (commands, conventions) kept in memory
- [ ] Conflicts Auto Memory vs AGENTS.md documented

## Workaround Audit
- [ ] Every negative recommendation ("avoid", "do not use") has a source
- [ ] Workarounds without BUG### / PR / test > 30 days marked `stale-workaround`
- [ ] No workarounds fossilized as permanent rules

## Project Boundary
- [ ] Stack in memory (.NET version, ORM) matches `global.json` / `.csproj`
- [ ] Build/test commands are current for this repo
- [ ] No mentions of technologies absent from the project

## Stale References
- [ ] No references to deleted files
- [ ] No mentions of outdated technologies (not matching global.json)
- [ ] Build/test commands are current

## Todo Graveyard
- [ ] All "Consider", "Need to", "Should", "TODO", "Eventually" found
- [ ] Items without ticket/PR and older than 30 days marked for archive
- [ ] Items with ticket marked `tracked`

## Observation Confidence
- [ ] "Preferences" / "conventions" have explicit source (PR, commit, human)
- [ ] Preferences without source and older than 60 days marked `unverified`
- [ ] No one-shot decisions generalized as team rules

## Canonical Boundaries
- [ ] Architecture — only in AGENTS.md
- [ ] Practice — only in Auto Memory
- [ ] No mixing of layers
