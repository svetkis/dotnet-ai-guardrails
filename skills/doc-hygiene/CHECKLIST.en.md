# Doc Hygiene Checklist

## Hierarchy
- [ ] All `AGENTS.md` are hierarchically consistent
- [ ] Deep overrides are explicitly marked and justified
- [ ] No cyclic contradictions (root vs module vs submodule)

## Internal Contradictions
- [ ] No MUST/FORBIDDEN pairs contradicting each other in the same file
- [ ] Conflicts are resolved or marked for discussion

## Code Drift
- [ ] Every AGENTS.md rule has a guardrail in code or CI
- [ ] Mentioned skills/tests exist in `skills/`, `tests/`
- [ ] Numbered Decisions from AGENTS.md found in code

## Dead Rules
- [ ] Every MUST/FORBIDDEN has enforcement (test, compiler, linter, CI)
- [ ] Rules without enforcement > 90 days marked `dead-rule`
- [ ] For each dead rule — decision: add guardrail or remove

## Cross-Agent
- [ ] All `docs/agents/*.md` are consistent with root `AGENTS.md`
- [ ] Pipeline descriptions are identical for all agents (format differs, substance does not)
- [ ] No links to deleted skills/modules

## README & Changelog
- [ ] Build commands are current
- [ ] CHANGELOG covers latest release
- [ ] No stale links

## Size Budget
- [ ] Root AGENTS.md ≤ 200 lines (warning > 150)
- [ ] Module-level AGENTS.md ≤ 80 lines
- [ ] If exceeded — there is a plan to split or refactor
