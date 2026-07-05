# Trap: Silent Breakdown

## Scenario

The agent optimizes read queries for performance. Adds `.AsNoTracking()` to all queries indiscriminately, without understanding the difference between read-path and write-path.

```csharp
// Agent optimized "ticket list"
var tickets = await dbContext.Tickets
    .AsNoTracking()  // ✅ OK here — pure read
    .ToListAsync();

// But then copied the same pattern into a command
var ticket = await dbContext.Tickets
    .AsNoTracking()  // ❌ HORROR! Change tracking disabled
    .FirstAsync(t => t.Id == id);

ticket.Resolve();     // Changing status
await dbContext.SaveChangesAsync();  // Silently not saving! 0 rows affected
```

## Why InMemory Tests Swallow It

The EF Core InMemory provider **does not emulate change tracking**. `SaveChanges()` always "succeeds", even with `AsNoTracking`.

## Consequences

- CI is green
- Unit tests pass
- In production, write fails for 21 hours
- A bug without an exception is the most expensive one

## Solution

1. **AGENTS.md** — explicit rule: `AsNoTracking` only in read-path with `.Select()`
2. **NBomber** — run read + write mix under load. $Max$ write latency spikes or failed requests appear
3. **Integration tests** — only on a real DB (TestContainers), no InMemory for logic

## Pattern

See `tests/patterns/LoadTest.cs`
