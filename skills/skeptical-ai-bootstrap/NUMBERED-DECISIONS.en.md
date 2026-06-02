# Numbered Decisions — Conscious Deviation Registry Template

> **Purpose:** Document every conscious deviation from the "standard" with a number and rationale, so the agent doesn't try to "fix" it.  
> **Consumer:** Human (Tech Lead) writes, agent reads comments in code.  
> **Result:** Registry of decisions like `PERF-###`, `DB-###`, `AUD-###` that are checked by an architecture uniqueness test.

---

## Why This Is a Separate File

`ARCHITECTURE-INVENTORY.md` is the "terrain map" (C4, assemblies, stack).  
And `NUMBERED-DECISIONS.md` is the **compromise journal**: why there is no index here, why `QueryFilter` was removed, why `SELECT *` is justified in this place.

Without such a journal, the agent will see "strange" code in 3 months and roll back the optimization.

---

## Record Template

```markdown
### {PREFIX}-###: {Short Name}
**Date:** YYYY-MM-DD  
**Author:** @username  
**Context:** {why we came to this decision}  
**Decision:** {what exactly we did}  
**Consequences:** {what will break if rolled back}  
**Where in code:** {file:line}
```

---

## Example

```markdown
### PERF-022: QueryFilter on SoftDelete Removed
**Date:** 2026-03-15  
**Author:** @lead  
**Context:** QueryFilter added a JOIN to users via Workspace.Owner.DeletedAt in every EXISTS subquery. Under load — 400ms degradation.  
**Decision:** Removed `HasQueryFilter(s => !s.IsDeleted)`. Soft delete implemented explicitly in queries.  
**Consequences:** An agent seeing `HasQueryFilter` in other configs will try to "fix" this. The number stops it.  
**Where in code:** `src/Infrastructure/Persistence/Configuration/SlotConfiguration.cs:31`
```

In code, a brief comment is left next to the decision:

```csharp
// PERF-022: QueryFilter removed — JOIN added 3ms to every query, see docs/NUMBERED-DECISIONS.md
builder.HasQueryFilter(s => !s.IsDeleted); // REMOVED — see PERF-022
```

---

## Naming Rules

| Prefix | What we document | Checked by test |
|--------|------------------|-----------------|
| `PERF-###` | Optimization, deviation from standard EF | `ArchitectureRules.cs` (ID uniqueness) |
| `DB-###` | DB schema decision (data type, index) | `ArchitectureRules.cs` (ID uniqueness) |
| `AUD-###` | Audit or logging decision | `ArchitectureRules.cs` (ID uniqueness) |
| `ARCH-###` | Layer or module boundary decision | Add to test yourself |
| `SEC-###` | Security rule exception (public webhook) | Add to test yourself |

> **Uniqueness test:** `ArchitectureRules.cs` template checks that `PERF-###`, `DB-###`, `AUD-###` are unique across the codebase. Duplicate = build failure. Prefixes `ARCH-###` and `SEC-###` — extensions; add them to the regex test (`(PERF|DB|AUD|ARCH|SEC)-\d{3}`) as needed.

---

## Where to Store in Project

Save this file next to `AGENTS.md` and `ARCHITECTURE-INVENTORY.md`:

```
docs/
  AGENTS.md
  ARCHITECTURE-INVENTORY.md
  NUMBERED-DECISIONS.md   ← here
```

If there are many decisions — they can be split by domain: `docs/decisions/PERF.md`, `docs/decisions/DB.md`.

---

> **Principle:** The comment `// PERF-022` in code stops the agent. And this file explains to a human why.
