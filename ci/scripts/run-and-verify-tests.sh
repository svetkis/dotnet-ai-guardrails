#!/usr/bin/env bash
# run-and-verify-tests.sh — single source of test-running logic.
#
# Runs one test project ONCE via `dotnet run --project`, then verifies:
#   1. tests actually ran (no "0 tests ran" / "no tests found" / "discovered: 0")
#   2. the runner produced a result line (passed/failed/skipped/total)
#   3. exit code and failure count match the expected mode
#
# Usage:
#   ./run-and-verify-tests.sh <test.csproj>                  # expect all tests to pass
#   ./run-and-verify-tests.sh <test.csproj> --expect-failure # traps: tests MUST fail
#
# Local helpers `run-tests.sh` and `verify-tests.sh` delegate to this script.
set -u

if [ $# -lt 1 ]; then
    echo "Usage: $0 <test.csproj> [--expect-failure]"
    exit 2
fi

PROJ="$1"
MODE="${2:-}"

if [ ! -f "$PROJ" ]; then
    echo "ERROR: project file not found: $PROJ"
    exit 2
fi

echo "========================================"
echo "Running tests: $PROJ ${MODE}"
echo "========================================"

TEST_OUTPUT=$(dotnet run --project "$PROJ" --configuration Release 2>&1)
EXIT_CODE=$?
echo "$TEST_OUTPUT"

fail() {
    echo "ERROR: $1"
    exit 1
}

# GUARDRAIL: "0 tests ran" with exit code 0 must not look green.
if echo "$TEST_OUTPUT" | grep -qi "0 tests ran\|no tests found\|discovered: 0"; then
    fail "Tests did not run in $PROJ (0 tests)."
fi

if ! echo "$TEST_OUTPUT" | grep -qi "passed\|failed\|skipped\|total:"; then
    fail "Cannot determine test results for $PROJ. Test runner may be misconfigured."
fi

if [ "$MODE" = "--expect-failure" ]; then
    # GUARDRAIL: traps project — tests MUST fail; a green run means guardrails broke.
    if [ "$EXIT_CODE" -eq 0 ]; then
        fail "Traps tests PASSED. Guardrails are broken — traps are no longer caught."
    fi
    if ! echo "$TEST_OUTPUT" | grep -q "failed:"; then
        fail "Expected test failures, but got an unexpected error (crashed or did not run)."
    fi
    echo "OK: Traps correctly caught by guardrails (exit code $EXIT_CODE)."
    exit 0
fi

if [ "$EXIT_CODE" -ne 0 ]; then
    fail "Test run failed in $PROJ (exit code $EXIT_CODE)."
fi
if echo "$TEST_OUTPUT" | grep -q "failed: [1-9]"; then
    fail "Tests failed in $PROJ."
fi

echo "OK: Tests were executed and passed in $PROJ."
exit 0
