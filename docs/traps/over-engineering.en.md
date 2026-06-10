# Trap: Over-Engineering

## Scenario

The agent implements a feature but builds an architectural cathedral instead of a simple solution:

- Creates `IValidationStrategy<T>` with 3 implementations to validate 2 fields
- Introduces CQRS + Read Model + Projection for an order list with 5 columns
- Wraps `HttpClient` in 4 layers for an external API call: `Factory` → `Provider` → `Service` → `Manager`
- Creates `BaseEntity<TId>` with 8 generic constraints for a `User` with `Id`, `Name`, `Email`

```csharp
// Agent: "Need to get order by id"
// Before (simple):
var order = await db.Orders.FindAsync(id);

// After (architectural cathedral):
var query = new GetOrderByIdQuery(id);
var handler = _mediator.Send(query);
var result = await _pipelineBehavior.Handle(handler, CancellationToken.None);
var dto = _mapper.Map<OrderResponseDto>(result.Value);
```

## Consequences

- **Reading time:** a junior developer spends a day to understand how `FindAsync` works
- **Debugging time:** a validation bug hides behind 3 interfaces and 2 factories
- **Compilation time:** generic nesting slows down IntelliSense and build
- **Testing time:** to check `a + b > 0`, you need to mock 5 dependencies
- **AI degradation:** the next agent, seeing "beautiful" code, adds yet another abstraction layer

## Why Agents Over-Engineer

- **Training data:** training corpora contain more "correct" Clean Architecture examples than simple scripts
- **Pattern recognition:** the agent sees `Order` → automatically generates `IOrderRepository`, `OrderService`, `OrderManager`
- **Hallucination of scale:** the agent doesn't know the project has 10 users and introduces Event Sourcing "for growth"
- **Lack of context:** the agent doesn't see that the neighboring feature was done in 5 lines and does its own in 500

## Why Automated Tests Are Not Enough

You can count interfaces or generic nesting depth, but **complexity is semantics, not syntax**:

```csharp
// An automated test won't understand this is overkill:
public interface IBookingValidationStrategy<TRequest, TResult, TContext>
    where TRequest : class, IRequest<TResult>
    where TResult : class
    where TContext : ValidationContext<TRequest>
{
    Task<TResult> ValidateAsync(TRequest request, TContext context, CancellationToken ct);
}
```

The test will say "many generic parameters" but won't explain why they exist.

## Solution

### 1. Simplicity Audit — Persona Auditor
An agent runs once per sprint with a simplicity checklist. See `templates/skills/simplicity-audit/SKILL.md`.

Checklist:
- [ ] Interface with one implementation — can it be replaced with a class?
- [ ] CQRS/Event Sourcing — is there a requirement for read/write separation or audit?
- [ ] Generic pipeline — how many parameters? Can it be collapsed?
- [ ] DTO nesting — how many levels? Does the client use all fields?
- [ ] async void — does any exist? Replace with Task?
- [ ] Methods with > 5 parameters — extract into a DTO?

### 2. Code Review: "Explain to a Junior" `[ADAPT]`
Add to your `templates/skills/code-review/CHECKLIST.md`:
> If a solution cannot be explained to a junior developer in 5 minutes, it is too complex.

### 3. AGENTS.md: "Simplicity > Pattern" Rule `[ADAPT]`
Add to your project's root `AGENTS.md`:

```markdown
## Simplicity vs Pattern
- Prefer `if/else` over `IStrategy` while branches < 3
- Prefer `db.Orders.Where(...)` over `IRepository<Order>` while there are no tests for DB replacement
- Prefer record/DTO over `IResponseMapper<TDomain, TDto>` while mapping is trivial
- Any abstraction must have **two** implementations or **one** compelling reason (testing, DI)
- async void is forbidden outside framework event handlers
- Method with > 5 parameters → extract into a parameter object
```

### 4. Metric: Interface-to-Class Ratio
An architectural test counts the interface-to-class ratio in a layer:

```csharp
// If Application > 0.8 — alert
var interfaces = assembly.GetTypes().Count(t => t.IsInterface);
var classes = assembly.GetTypes().Count(t => t.IsClass && !t.IsAbstract);
var ratio = (double)interfaces / classes;
```

### 5. Objective Metrics + Dead Guardrail
Automated tests catch measurable complexity **only if that pattern actually occurs in your codebase**:

- `GenericParameters_ShouldNotExceed_3` — public types with > 3 generic parameters
- `MethodNames_ShouldNotExceed_40Chars` — method names > 40 characters
- `MethodParameters_ShouldNotExceed_5` — methods with > 5 parameters
- `AsyncVoid_ShouldNotExist` — async void in production code
- `FileLength_ShouldNotExceed_300Lines` — files > 300 effective lines

> **But:** if generic > 3, inheritance > 3, or nested Func **never occur** in your project, these checks are a **dead guardrail**. It creates a false sense of security, wastes CI time, and dilutes attention.
>
> ```csharp
> // You added SimplicityGuardTest with 8 checks
> // But in your project:
> // - Generic > 3 parameters — never happened
> // - Inheritance depth > 3 — never happened
> // - Nested Func — never happened
> // 
> // Result: 27 tests, 0 failures, 0 value.
> // This is architectural dead code — exactly what the guardrail should catch.
> ```
>
> **Rule:** keep a guardrail only if it has caught at least one real bug. Otherwise — delete it.

### 6. "No Abstraction Without Pain" Rule
The project adopts the principle: an abstraction is introduced only when there is **already** pain from its absence (tests break, code duplicates, implementation replacement is needed).

Not: "might come in handy". Only: "already hurts without it".

## Pattern

See `templates/skills/simplicity-audit/SKILL.md` and `templates/skills/simplicity-audit/CHECKLIST.md`
