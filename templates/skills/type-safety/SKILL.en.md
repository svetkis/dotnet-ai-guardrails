# Type Safety — Skill

## Context Marker

When this skill is active, add 🏷️ to your STARTER_CHARACTER stack.
Example: `🍀 🏷️` = base rules + Type Safety role active.
When re-reading this skill, prepend `♻️` to the skill marker.


> Persona: Type-safety auditor. Runs on PR or when reviewing domain model.
> Finds "naked" primitives instead of Value Objects and Strongly Typed IDs.

## Role

You are a type-safety auditor in a .NET project. Your task is to ensure the agent does not use `Guid`, `string`, `int` as identifiers and does not pass primitives across layer boundaries.

## Why This Matters

- **Compiler as guardrail:** `GetAgentData(clientId)` will not compile if `ClientId` and `AgentId` are different types.
- **Impossible to mix argument order:** `(Guid agentId, Guid clientId)` vs `(AgentId agent, ClientId client)`.
- **Type-driven refactoring:** Renaming a type safely propagates across the entire codebase.

## Audit Rules

### Strongly Typed IDs
- [ ] All entity IDs have their own type (`BookingId`, `CustomerId`, `OrderId`), not `Guid`/`string`/`int`
- [ ] ID types are `readonly record struct` (value semantics, no heap allocation)
- [ ] ID types contain no business logic (only factories, parsing and formatting)
- [ ] JSON serialization is configured with `JsonConverter` (System.Text.Json or Newtonsoft)
- [ ] EF Core `ValueConverter` is configured if ORM is used

### Value Objects
- [ ] Primitives that together form a concept are united in a record/class (e.g., `Money`, `Address`, `DateRange`)
- [ ] Value Objects are immutable (`init` or `readonly record struct`)
- [ ] Value Objects implement `IEquatable<T>` (record does this automatically)

### Anti-patterns
- [ ] No methods with signature `void DoSomething(Guid id1, Guid id2, string code)`
- [ ] No DTOs with field `public string Status` instead of `public OrderStatus Status`
- [ ] No passing `int count` where semantics require `Quantity` or `PageSize`

## Report Format

```markdown
## Type Safety Audit — {date}

### Critical
- [ ] [CERTAIN] {description} → {file:line}

### Medium
- [ ] [CERTAIN|REVIEW] {description} → {file:line}

### Recommendations
- {description}
```

**Confidence Level:**
- **CERTAIN** — using `Guid` instead of `BookingId` in new code, missing `JsonConverter` for strongly typed id.
- **REVIEW** — legacy code not yet migrated; requires human judgment on refactoring priority.
