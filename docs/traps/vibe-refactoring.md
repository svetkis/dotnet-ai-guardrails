# Trap: Vibe Refactoring

## Scenario

The agent decides to "clean up the code":
- Deletes 3000 lines of "unused" code
- Removes attributes that "don't affect anything"
- Changes architecture because "it's better this way"

```csharp
// Agent: "[SensitiveData] is not used at runtime, I'll remove it for cleanliness"
// Before:
[SensitiveData]
public string Email { get; init; }

// After:
public string Email { get; init; }
```

## Consequences

- The PII field is no longer marked — logs start leaking
- Validation was removed during refactoring, but no one noticed
- Compliance tests fail, but the cause is unclear

## Solution

1. **NetArchTest** — regulates what is and isn't allowed. Forbidding `FindAsync` in read-path, forbidding direct Infrastructure dependencies from Api
2. **Ratchet tests** — use reflection to count public types in a layer and tests. Count decreases → test fails
3. **Code Review Agent** — a separate agent checks the diff before commit

## Pattern

See `tests/patterns/RatchetTest.cs` and `tests/patterns/ArchitectureRules.cs`
