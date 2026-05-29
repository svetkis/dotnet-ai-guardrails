# Trap: Log Leak

## Scenario

The agent adds logging for debugging and doesn't think about sensitive data:

```csharp
// Agent: "I'll add a log to see why it fails"
_logger.LogInformation("User {Email} failed login from {Ip}", user.Email, ip);

// Or even worse — string interpolation:
_logger.LogError($"Payment failed for {order.Phone}, card: {order.CardLast4}");
```

## Consequences

- Emails, phones, IP addresses leak into the logging system (Elastic, Kibana, Seq)
- Logs are often more widely accessible than the DB (SRE, support, external systems)
- GDPR / compliance violation — fines
- Tokens and session IDs in logs = potential session compromise

## Why Standard Layers Don't Catch It

| Layer | Why it doesn't catch |
|-------|----------------------|
| Compiler | `LogInformation(string, object[])` — valid signature |
| Architecture | NetArchTest doesn't see string argument contents |
| Tests | Unit tests check logic, not what goes into `ILogger` |
| Code Review | The agent-reviewer sees "logging added" and doesn't check arguments |
| E2E | Application works, logs are written |

## Solution

### Level 1. Compile-time guard (Roslyn / Analyzer)

Custom `DiagnosticAnalyzer` forbids `Log*` with interpolation (`$"..."`) and marked types:

```csharp
// Analyzer emits warning/error:
_logger.LogError($"Failed for {user.Email}"); // ERROR: interpolated string in logging
```

### Level 2. Attribute-driven inventory (Ratchet)

All PII fields are marked with `[SensitiveData]`:

```csharp
public record UserDto(
    [SensitiveData] string Email,
    [SensitiveData] string Phone,
    string Name
);
```

Arch test checks:
1. All properties with `*Email*`, `*Phone*`, `*Password*` in the name have `[SensitiveData]`
2. The number of `[SensitiveData]` properties does not decrease (ratchet)
3. `Log*` calls do not pass parameters of types containing `[SensitiveData]`

### Level 3. Runtime redaction (Serilog / Middleware)

```csharp
// Serilog destructuring policy
public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyFactory,
        out LogEventPropertyValue result)
    {
        var type = value.GetType();
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null);

        // Redact marked properties
        ...
    }
}
```

## Pattern

See `tests/patterns/PiiGuardTest.cs` and `docs/solutions/ai-patterns.md` (pattern #9)
