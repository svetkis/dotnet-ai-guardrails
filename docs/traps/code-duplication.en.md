# Trap: Code Duplication

## Scenario

The agent implements a new feature without noticing that similar logic already exists:

- Copies validation from one service to another
- Duplicates discount/fee calculation in an API controller
- Inserts an order status check into 3 different handlers

```csharp
// Agent: "I need to check that the order is confirmed"
// Service 1:
if (order.Status == BookingStatus.Confirmed && order.Total > 0)

// Service 2 (copied):
if (order.Status == BookingStatus.Confirmed && order.Total > 0)

// Service 3 (copied):
if (order.Status == BookingStatus.Confirmed && order.Total > 0)
```

## Consequences

- Fixing a bug in one place does not fix it in others
- The agent changes logic in one service but is unaware of the duplicates
- Compliance rules diverge (in one place `>= 100`, in another `> 100`)
- Refactoring regressions — "I fixed it" (but only in one place)

## Why the agent duplicates

- The agent looks at the diff, not the whole codebase
- Context window is insufficient to search for similar fragments
- "Vibe coding" — faster to copy than to find a shared abstraction

## Why automated tests are not enough

Regex scanning only catches **literal** duplication. Reality is worse:

```csharp
// Service 1
if (order.Status == BookingStatus.Confirmed)

// Service 2
if (order.IsConfirmed())

// Service 3
order.EnsureConfirmed();
```

This is the same business rule, but the automated test **will not see it**.

## Solution

### 1. Automated test: Literal Duplication Guard
Regex scanning for literal copying. Catches agent copy-paste. See `tests/patterns/DuplicationGuardTest.cs`.

### 2. Code Review: Semantic Duplication in diff
The reviewer agent checks: if validation/calculation was added in the PR — does similar logic already exist in other services? See `skills/code-review/CHECKLIST.md`.

### 3. Tech Debt Audit: Semantic Duplication across codebase
An agent (and regex) cannot recognize **semantic** duplication across the entire codebase. This is the task of the Tech Lead persona-audit, run once per sprint. See `skills/tech-debt-audit/SKILL.md`.

Audit checklist:
- [ ] New validation/status calculation — does similar logic exist in other services?
- [ ] Can the rule be extracted into a Domain Service / Value Object?
- [ ] Is there an existing `BR-###` (numbered business rule) that covers this case?

### 4. Numbered business rules `BR-###`
Like `PERF-###` / `DB-###`, but for business logic:

```csharp
// BR-001: An order is considered confirmed only at Confirmed status and non-zero total
public static class BookingRules
{
    public static bool IsConfirmed(this Booking b) =>
        b.Status == BookingStatus.Confirmed && b.Total > 0;
}
```

If an agent encounters `BR-###` in code — it must use the existing method rather than writing a new check.

### 5. Domain Services
Move business rules into the domain; prohibit hardcoding in Application/API.

## Pattern

See `tests/patterns/DuplicationGuardTest.cs`
