#!/bin/bash
# GUARDRAIL: Проверяем, что dotnet run --project реально запустил тесты.
# Ловит ситуацию, когда тестовый раннер сломан и выдаёт 0 tests ran с exit code 0.
#
# Использование:
#   ./verify-tests.sh <path-to-test.csproj>     # проверить один проект
#   ./verify-tests.sh                           # проверить все проекты в tests/

set -e

verify_project() {
    local proj="$1"
    echo "Verifying tests ran for: $proj"

    TEST_OUTPUT=$(dotnet run --project "$proj" --configuration Release 2>&1)

    echo "$TEST_OUTPUT"

    # Проверяем, что в выводе есть упоминание о запущенных тестах
    if echo "$TEST_OUTPUT" | grep -qi "0 tests ran\|no tests found\|discovered: 0"; then
        echo "ERROR: Tests did not run in $proj! Output shows 0 tests."
        exit 1
    fi

    # Проверяем, что есть упоминание о passed/failed (TUnit/xUnit/NUnit выводит статистику)
    if ! echo "$TEST_OUTPUT" | grep -qi "passed\|failed\|skipped\|total:"; then
        echo "ERROR: Cannot determine test results for $proj. Test runner may be misconfigured."
        exit 1
    fi

    echo "OK: Tests were executed and results detected for $proj."
}

if [ -n "$1" ]; then
    # Один проект
    verify_project "$1"
else
    # Все проекты в tests/
    if [ ! -d "tests" ]; then
        echo "ERROR: tests/ directory not found. Pass a .csproj path or adapt this script."
        exit 1
    fi

    FOUND=0
    while IFS= read -r -d '' proj; do
        FOUND=1
        verify_project "$proj"
    done < <(find tests -name "*.csproj" -print0)

    if [ "$FOUND" -eq 0 ]; then
        echo "ERROR: No test projects found in tests/."
        exit 1
    fi
fi
