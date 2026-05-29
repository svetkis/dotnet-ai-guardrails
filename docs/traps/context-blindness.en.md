# Trap: Context Blindness

## Scenario

The agent sees only the files it is editing. The context window is limited, and the agent:
- Does not see that the new endpoint is not covered by authorization
- Does not see that changing the DTO will break 3 other services
- Does not see that a "simple optimization" broke the Telegram integration

```csharp
// Agent added a new field to Response DTO
public record TicketDto(
    int Id,
    string Title,
    string InternalNotes  // ❌ Oops, this private field leaked into the API
);
```

## Why This Is a Systemic Problem

The agent cannot hold the entire codebase in its head. It optimizes locally, but the consequences are global.

## Solution

1. **E2E MCP** — make the agent poke the system itself. Telegram bot, API client — real scenarios
2. **Batch audits** — create narrow personas:
   - **Security** — sees data leaks that the agent missed
   - **DBA** — sees N+1 and missing indexes
   - **UX** — sees unclear errors and strange texts
3. **Scheduled runs** — audits don't wait for PR, they systematically search for holes

## Result

19 bugs found by shifting focus. The agent didn't see them because it was looking at functionality. The auditors were looking at risks.

## Pattern

See `skills/security-audit/`, `skills/dba-audit/`, `skills/api-design-audit/`, `skills/bot-audit/`
