# Trap: Stale Stack

## Scenario

The agent generates code based on training data, not on the actual state of the ecosystem:

- Uses .NET 10 preview, although the team standard is stable SDK only
- Pins EF Core 8 in a .NET 9 project, although EF Core 9 is available
- Suggests `Microsoft.Extensions.Caching.Memory` 6.x instead of the current 9.x
- In the frontend part: React 17 + class components instead of functional + hooks
- Uses packages with `-preview`, `-rc`, `-beta` flags without explicit agreement

```csharp
// Agent: "Here is an example with .NET 10 Preview 3"
// global.json
{
  "sdk": {
    "version": "10.0.100-preview.3",
    "rollForward": "latestFeature"
  }
}

// PackageReference — версия из training cutoff
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

## Consequences

- Security patches are not applied automatically (preview packages often don't receive updates)
- New platform features are unavailable (e.g., C# 13 features in .NET 9)
- Extra transitive dependencies from old packages
- Incompatibility with the rest of the team's stack
- The agent writes code with outdated APIs that are already deprecated

## Why Standard Layers Don't Catch It

| Layer | Why it doesn't catch |
|-------|----------------------|
| Compiler | Code compiles — preview SDK is valid |
| Architecture | NetArchTest doesn't check package versions |
| Tests | Unit tests check logic, not the manifest |
| Code Review | The agent-reviewer also relies on the training cutoff |
| E2E | Application works, but with deprecated dependencies |

## Solution

1. **VersionAuditTest** — test scans `global.json`, `*.csproj`, `package.json`:
   - Forbids `preview`, `rc`, `beta` in `global.json` without an explicit whitelist
   - Checks that `TargetFramework` matches the team standard
   - Scans `PackageReference` for outdated major versions

2. **SKILL.md version-audit** — periodic audit:
   - Compares `PackageReference` with current versions via `nuget.org` API or `dotnet list package --outdated`
   - Checks that frontend dependencies don't lag more than 1 major version

3. **AGENTS.md rule** — "Do not use preview versions without explicit agreement in PR"

## Pattern

See `tests/patterns/VersionAuditTest.cs` and `skills/version-audit/`
