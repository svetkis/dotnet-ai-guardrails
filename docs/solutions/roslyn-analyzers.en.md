# Roslyn Analyzers as Compile-Time Guardrails

> Regex searches strings. Roslyn understands C#.
> For source-level C# guardrails, the default choice is a Roslyn analyzer.

## When To Choose Roslyn

Use an analyzer when the rule depends on C# meaning:

| Rule | Why Roslyn |
|------|------------|
| Forbid raw `Guid Id` in Domain | Needs property type and namespace |
| Forbid `FindAsync()` in read-path | Needs real invocation, not comments, and write-path context |
| Forbid allocations in `[HotPath]` | Needs attributes, `new`, array creation, boxing, `async` |
| Public DTO must not expose internal entity | Needs symbols and return types |
| Method must accept `CancellationToken` | Needs signature and async usage |

## When Not Roslyn

| Check | Tool |
|-------|------|
| Dependencies between assemblies / layers | NetArchTest |
| Cycles between slices/modules | ArchUnitNET |
| Unique `PERF-###`, `DB-###`, `AUD-###` in markdown/config/code comments | Regex or parser |
| `.csproj`, `.yml`, `package.json`, lock files | Structured parser or regex |

## Working Example

`examples/DemoProject/src/DemoProject.Analyzers/`:

- `StronglyTypedIdAnalyzer.cs`
  - `SAE001`: primitive `*Id` property in Domain
  - `SAE002`: raw `Guid *Id` parameter in Domain
- `HotPathAnalyzer.cs`
  - `SAE003`: allocation in `[HotPath]`
  - `SAE004`: `async` state machine in `[HotPath]`
  - `SAE005`: boxing in `[HotPath]`

Project hookup:

```xml
<ProjectReference Include="..\DemoProject.Analyzers\DemoProject.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## Minimal Agent Process

1. State the bug class: which specific agent bug this catches.
2. Write 2-3 tiny code examples: should trigger / should not trigger.
3. Create a `DiagnosticDescriptor` with an `SAE###` ID.
4. Register a syntax or operation action.
5. Use `SemanticModel` when the rule depends on types or symbols.
6. Hook the analyzer with `OutputItemType="Analyzer"`.
7. Set severity: error for safety/correctness, warning for performance guidance.

## Repository Rule

Regex over `.cs` is only a temporary spike or fallback. If a C# rule has stabilized and should protect the team permanently, promote it to a Roslyn analyzer.
