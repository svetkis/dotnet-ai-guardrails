---
name: memory-hygiene
description: >
  Grooming of agent's Auto Memory. Detects duplicates, stale notes,
  workarounds-fossilized-as-rules, and contradictions between flat agent
  memory and hierarchical guardrails (AGENTS.md).
---

> **Repo-internal / for methodology archive.** This skill describes a methodological guardrail inside the `dotnet-ai-guardrails` repository. The methodological core (grooming flat agent memory, deduplication, hierarchical drift) applies to any project, but the concrete memory sources and formats are illustrations, not a universal template.

# Memory Hygiene Agent

## Context Marker

When this skill is active, add 🧹 to your STARTER_CHARACTER stack.
Example: `🍀 🧹` = base rules + Memory Hygiene role active.
When re-reading this skill, prepend `♻️` to the skill marker.


## Role

You are a memory grooming agent. Your task is to clean up flat notes
that the agent has accumulated about the project and eliminate duplication
with hierarchical guardrails.

## Preconditions

- **Auto Memory** is flat. Bound to the git repository root. Does not
  understand folder hierarchy. The agent maintains it automatically.
- **AGENTS.md** is hierarchical. Context-dependent for each folder.
  Designed by the developer.
- **Auto Memory ≠ AGENTS.md**. They are orthogonal.

## Anti-patterns

| Problem | Why it's bad |
|---------|-------------|
| Duplicates of AGENTS.md in Auto Memory | Agent pulls outdated copy instead of actual guardrail |
| Architectural decisions in Auto Memory | Flat memory cannot distinguish module context (Ordering vs Payment) |
| Contradicting notes | One session remembered Dapper, another — EF Core |
| Stale file references | Agent suggests modifying a file that no longer exists |
| **Workaround fossilization** | Agent remembered a workaround as best practice. Bug was fixed — workaround persists in memory |
| **Cross-project contamination** | Facts from another repo leaked into current project memory (stack, ORM, conventions) |
| **TODO accumulation** | "Consider…", "Need to…" accumulate for months and interfere with prioritization |
| **One-shot generalization** | One-off PR decision generalized as team preference |

## Process

### Phase 1: Inventory
1. Find all sources of flat memory:
   - `.claude/CLAUDE.md` (if used as flat memory)
   - `.kimi/skills/README.md`
   - `.serena/memories/`
   - Any `.md` in the root that the agent uses as context

### Phase 2: Semantic Deduplication

// TRAP: Agents rephrase rather than copy verbatim.
// GUARDRAIL: Group by intent, not by verbatim text.

- Keyword-clustering by intent (not string fuzzy-match)
- Grouping: "use `.Select()`" and "projections are mandatory" — same intent
- Flag: "This note duplicates `rules/AGENTS_TEMPLATE.md` §3.2 — recommend deletion"

### Phase 3: Hierarchical Drift Detection
- Compare Auto Memory with nearest `AGENTS.md` for each module
- Flag contradictions: Auto Memory says X, but `src/Ordering/AGENTS.md` says Y

### Phase 3a: Workaround Audit

// TRAP: Agent applied a workaround, memorized it, bug was fixed — workaround persists.
// GUARDRAIL: Every negative recommendation without a bug/PR reference is suspicious.

- Find notes with negative recommendations ("avoid…", "do not use…")
- Check: is there a `BUG###_` test or PR confirming relevance?
- If source > 30 days and no confirmation — mark `stale-workaround`

### Phase 3b: Project Boundary Check

// TRAP: Agent worked on a Dapper project yesterday, today gives advice from that memory.
// GUARDRAIL: Stack and commands in memory are verified against current repo's `global.json` / `.csproj`.

- Verify mentioned stack (.NET version, ORM, framework) against `global.json`
- Check build/test commands match current repo
- Flag: "Dapper mentioned in memory, but `.csproj` only has EF Core — cross-project contamination?"

### Phase 4: Stale Reference Cleanup
- Find mentions of files, types, namespaces that no longer exist in code
- Find outdated build/run commands

### Phase 4a: Todo Graveyard

// TRAP: Agents write "Consider adding caching", "Need to refactor" and forget.
// GUARDRAIL: "Consider/Need to/TODO" without a ticket and older than 30 days — archive.

- Find all items with "Need to", "Consider", "Should", "TODO", "Eventually"
- If linked ticket/PR exists — keep, mark `tracked`
- If no source and > 30 days — mark `todo-graveyard`, recommend archive

### Phase 5: Observation Confidence

// TRAP: Agent saw a one-off PR decision and generalized it as team preference.
// GUARDRAIL: Preference without explicit source (PR #, commit, human instruction) — unverified.

- Find notes about "team preferences" (prefers, always, never, convention is)
- Check: is there a source (PR, commit message, explicit human instruction)?
- If source is missing or > 60 days old — mark `unverified-preference`

### Phase 6: Cleanup Recommendations

```markdown
## Memory Hygiene Report

### Semantic Duplicates
- [ ] `memory-07.md` ↔ `memory-12.md` — same intent (projections), merge

### Hierarchical Drift
- [ ] `memory-03.md`: "use EF Core" vs `src/Payment/AGENTS.md`: "Dapper only"

### Stale Workarounds
- [ ] `memory-09.md`: "Avoid ExecuteUpdateAsync on Order" — no BUG###, no PR, > 45 days

### Cross-project Contamination
- [ ] `memory-02.md`: mentions Dapper, but repo stack is EF Core

### Stale Notes
- [ ] `memory-05.md`: references deleted `LegacyPaymentService.cs`

### Todo Graveyard
- [ ] `memory-11.md`: "Consider repository pattern" — no ticket, 90 days

### Unverified Preferences
- [ ] `memory-04.md`: "Team prefers JSON over MessagePack" — source not found
```

## Output

- `.backlog/memory-hygiene-{date}.md`

## Key Rule

> Architectural decisions live in `AGENTS.md`. Practical details (commands,
> naming conventions) — in Auto Memory. If an architectural rule is found
> in Auto Memory — move it to hierarchical guardrail and delete from flat memory.