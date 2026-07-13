#!/usr/bin/env bash
# verify-tests.sh — local helper. Delegates to run-and-verify-tests.sh
# (single source of test-running logic).
#
# Usage:
#   ./verify-tests.sh <path-to-test.csproj>     # verify one project
#   ./verify-tests.sh                           # verify all projects in tests/
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ -n "${1:-}" ]; then
    exec "$SCRIPT_DIR/run-and-verify-tests.sh" "$1"
fi

if [ ! -d "tests" ]; then
    echo "ERROR: tests/ directory not found. Pass a .csproj path or adapt this script."
    exit 1
fi

FOUND=0
while IFS= read -r -d '' proj; do
    FOUND=1
    "$SCRIPT_DIR/run-and-verify-tests.sh" "$proj"
done < <(find tests -name "*.csproj" -print0)

if [ "$FOUND" -eq 0 ]; then
    echo "ERROR: No test projects found in tests/."
    exit 1
fi
