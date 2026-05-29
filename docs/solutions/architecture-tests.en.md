# Architecture Tests in .NET — Catching Violations Before Review

> 25+ tests in 4 files — catch violations BEFORE code review.

## Tools

| Approach | What it checks | Library |
|----------|----------------|---------|
| Assembly reflection | Dependencies, naming, inheritance | NetArchTest |
| Source scanning | Anti-patterns in code (regex over .cs files) | Custom helper |
| Type counting | Number of public types and tests does not decrease | Reflection |

---

## 1. Layered Architecture (Clean Architecture)

**File:** `ArchitectureTests.cs`

| Test | Rule |
|------|------|
| `Domain_ShouldNotDependOn_Application` | Domain does not know about Application |
| `Domain_ShouldNotDependOn_Infrastructure` | Domain does not know about Infrastructure |
| `Application_ShouldNotDependOn_Infrastructure` | Application does not depend on Infrastructure |
| `Api_ShouldNotDependOn_Infrastructure` | Api depends only via DI |

**Why:** One `using MyApp.Infrastructure` in Application — and the dependency leaks. The test breaks instantly.

---

## 2. Naming Conventions

**File:** `ArchitectureTests.Naming.cs`

| Test | Rule |
|------|------|
| `Interfaces_ShouldStartWith_I` | Interfaces in Domain and Application start with `I` |
| `Jobs_ShouldEndWith_Job` | Background jobs end with `Job` |
| `Dtos_ShouldBeRecords` | DTOs are records only (immutability) |

**Why:** Uniformity = codebase search. `grep *Job` finds all jobs.

---

## 3. Structural Rules + Regex Scanning

**File:** `ArchitectureTests.Structure.cs`

### EF Anti-patterns (source scanning)

Not only assembly reflection, but also **regex over .cs files**:

```csharp
// Looking for FindAsync() in services (forbidden in read-path)
var violations = ScanServicesForPattern(@"\.FindAsync\(", "*.cs", whitelist);
```

| Test | Rule |
|------|------|
| `Services_ShouldNotUse_FindAsync` | Forbid `FindAsync()` — loads full Entity. Whitelist for write-path |
| `QueryServices_ShouldNotUse_Include` | Forbid `.Include()` in QueryService — only `.Select()` projections |
| `FindAsync_Whitelist_ShouldNotBeStale` | Whitelist does not go stale — if a file no longer uses FindAsync, the test fails |

### Cache Safety

| Test | Rule |
|------|------|
| `CacheSet_ShouldAlwaysSpecifySize` | Every `cache.Set()` must specify size (SizeLimit on MemoryCache) |

### Decision Tracking

| Test | Rule |
|------|------|
| `PerfAndDbDecisions_ShouldHaveUniqueIds` | Optimization IDs (`PERF-###`, `DB-###`) are unique across the codebase |

---

## 4. Architectural Inventory Control

**File:** `ArchitectureTests.Ratchet.cs`

| Test | Rule |
|------|------|
| `PublicTypeCount_ShouldNotDecrease` | Number of public types in Application >= baseline. Cannot silently remove a service or DTO |
| `TestCount_ShouldNotDecrease` | Number of tests >= baseline. Catches "0 tests ran" |

**Why:** The agent likes to "clean up code" and remove "unused" services. Reflection by namespace catches this instantly.

---

## Whitelist with Staleness Check

Whitelist for exceptions (write-path) is itself checked: if a file from the whitelist no longer contains the pattern — the test fails. The agent cannot "clean up" code and leave a dead entry.

```csharp
var whitelist = new[]
{
    "BookingCommandService.cs:2 calls: write-path uses FindAsync for update",
    "MasterProfileService.cs:1 call: write-path entity load"
};
```

---

## How to Add a New Test (Template)

```csharp
// Reflection: type checking
[Test]
public async Task MyRule_ShouldBeEnforced()
{
    var result = Types.InAssembly(DomainAssembly)
        .That().ResideInNamespace("MyApp.Domain")
        .Should().BeSealed()
        .GetResult();

    await Assert.That(result.IsSuccessful).IsTrue();
}

// Scanning: anti-pattern search in code
[Test]
public async Task Services_ShouldNotUse_BadPattern()
{
    var violations = ScanServicesForPattern(
        @"\.BadMethod\(",
        "*.cs",
        whitelist: new[] { "LegitimateUse.cs:2" });

    await Assert.That(violations)
        .IsEmpty()
        .Because("BadMethod loads too much data");
}
```

---

## Pattern

See `tests/patterns/ArchitectureRules.cs`
