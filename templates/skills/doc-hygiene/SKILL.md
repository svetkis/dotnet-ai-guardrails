---
name: doc-hygiene
description: >
  Grooming of hierarchical project documentation. Checks consistency of
  AGENTS.md, README, docs/ and correspondence to actual code.
  Catches dead rules and guardrail bloat.
---

> **Repo-internal / for methodology archive.** This skill is intended for internal self-audit of the `dotnet-ai-guardrails` repository. It checks the integrity of its own ecosystem (`docs/agents/`, `rules/AGENTS_TEMPLATE.md`, `templates/skills/`, `examples/`). The methodological core (hierarchy consistency, code drift, dead rules, fact checking) can be adapted to another project, but the concrete paths and artifacts are specific to this repository.

# Doc Hygiene Agent

## Context Marker

When this skill is active, add 📝 to your STARTER_CHARACTER stack.
Example: `🍀 📝` = base rules + Doc Hygiene role active.
When re-reading this skill, prepend `♻️` to the skill marker.


## Role

You are a documentation grooming agent. You check that hierarchical
guardrails do not contradict each other and match the code.

## Scope

- `AGENTS.md` (root and subfolders)
- `README.md`, `README.en.md`
- `docs/agents/*.md`
- `docs/solutions/*.md`
- `CONTRIBUTING.md`
- `CHANGELOG.md`

## Anti-patterns

| Problem | Why it's bad |
|---------|-------------|
| Root AGENTS.md contradicts module one | Agent doesn't know which rule to follow |
| AGENTS.md requires X, but no guardrail for X in code | Rule is a dead letter |
| docs/agents/KIMI.md describes non-existent pipeline | Onboarding of a new agent starts with a lie |
| README is outdated relative to stack | Human developer loses trust |
| **AGENTS.md bloat** | Agent stops reading the file entirely due to size |
| **Dead rules** | Rule exists, but no enforcement (test, compiler, CI) |
| **Internal contradictions** | Two rules in one AGENTS.md contradict each other |

## Process

### Phase 1: Hierarchy Consistency
1. Root `AGENTS.md` → `rules/AGENTS_TEMPLATE.md` — no conflicts?
2. `src/{Module}/AGENTS.md` — do not contradict root?
3. Deep overrides: does deeper AGENTS.md override shallower one?
   Check that override is intentional and documented.

### Phase 1a: Internal Contradictions

// TRAP: Agent adds a rule to AGENTS.md without noticing it contradicts existing §2.1.
// GUARDRAIL: Every MUST / FORBIDDEN is cross-checked with other rules in the same file.

- Find rule pairs where one requires X and another forbids X
- Example: "All services must have interfaces" vs "Minimal API — static classes, no interfaces"
- Mark conflicts as `internal-contradiction`, require resolution

### Phase 2: Code Drift
1. `AGENTS.md` forbids `.FindAsync()` in read-path → is there a regex test?
2. `AGENTS.md` requires `BUG###_` tests → is there a convention in `tests/`?
3. Do mentioned modules/skills exist in `templates/skills/`, `tests/`?
4. Are Decision Guards (`PERF-###`) from AGENTS.md present in code?

### Phase 2a: Rule Vitality

// TRAP: AGENTS.md forbids "raw SQL without comment", but no test/analyzer enforces it. Agents quickly learn the rule is dead.
// GUARDRAIL: Every MUST/FORBIDDEN has enforcement: compiler, test, linter, or CI.

- For each MUST / FORBIDDEN find enforcement (test, compiler, linter, CI)
- Rules without enforcement > 90 days — mark `dead-rule`
- Recommend: either add guardrail or remove the rule

### Phase 3: Cross-Agent Docs
1. Is `docs/agents/KIMI.md` current relative to `AGENTS.md`?
2. No divergence in pipeline description between `docs/agents/CLAUDE-CODE.md` and `docs/agents/OPENCODE.md`?
3. Do all agents describe the same stack/versions?

### Phase 4: README & CHANGELOG
1. Does `README.md` contain current build commands?
2. Does `CHANGELOG.md` cover the latest release?
3. No links to deleted sections/skills?

### Phase 5: Size Budget

// TRAP: AGENTS.md grows to 500 lines. Agent reads the beginning, skips the middle, breaks on the end.
// GUARDRAIL: Hard or soft budget on size + suggestion to split.

- Count lines in root `AGENTS.md`
- If > 150 lines — warning (`approaching budget`)
- If > 200 lines — mark `size-budget-exceeded`, suggest splitting into module-specific files
- Count module-level AGENTS.md: if > 80 lines — suggest refactoring

### Phase 6: Report

```markdown
## Doc Hygiene Report

### Hierarchy
- [ ] `src/Payment/AGENTS.md` overrides "Dapper" to "EF Core" — documented?

### Internal Contradictions
- [ ] Root guardrail §X requires A, §Y forbids A

### Code Drift
- [ ] Guardrail requires X, but corresponding guardrail test / analyzer not found

### Dead Rules
- [ ] Guardrail forbids X — no enforcement found (> 90 days)

### Cross-Agent
- [ ] Agent documentation references deleted skill / artifact

### README
- [ ] Build command outdated

### Size Budget
- [ ] Root guardrail — 230 lines, exceeds budget of 200
```

## Output

- `.backlog/doc-hygiene-{date}.md`

## Key Rule

> AGENTS.md is the single source of truth for architectural guardrails.
> Everything else (docs/agents/, templates/skills/) — derived. If it diverges —
> update derived, not the root.
> A dead rule is worse than no rule. If there is no enforcement —
> remove it or add a guardrail.