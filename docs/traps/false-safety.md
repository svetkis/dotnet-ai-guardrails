# Trap: False Safety

## Scenario

The agent updates TUnit or changes `.csproj`. As a result, `dotnet test` silently outputs:

```
Build succeeded.
Test run finished: 0 tests ran
```

Exit code: 0. CI is green. Code gets merged.

## Why This Is Dangerous

For two weeks the team thinks everything is checked. In reality:
- A new bug is not caught
- Regression goes through
- The agent broke the runner settings

## Root Causes

- TUnit + .NET 10 + MTP: `dotnet test` doesn't always correctly run TUnit
- The agent removed `<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>`
- The agent renamed the test project, but CI still points to the old path

## Solution

1. **`dotnet run --project`** instead of `dotnet test`
2. **Verify script** — `ci/scripts/verify-tests.sh` parses output and checks that count > 0
3. **CI guardrail** — a separate step that fails if "0 tests ran"

## Related Traps

- [non-validating-tests](non-validating-tests.md) — the test-level instance of
  false safety: the test runs and is green, but its assertions cannot fail when
  the promised behavior breaks. Runner-level verification (this trap) does not
  detect it — use assertion reachability analysis and fault-injection checks.

## Pattern

See `tests/conventions/TUnit_Guide.md` and `ci/github-actions/safe-ci.yml`
