#!/bin/bash
# GUARDRAIL: Проверяем, что dotnet run --project реально запустил тесты.
# Ловит ситуацию, когда TUnit сломан и выдаёт 0 tests ran с exit code 0.

set -e

TEST_OUTPUT=$(dotnet run --project "$1" --configuration Release 2>&1)

echo "$TEST_OUTPUT"

# Проверяем, что в выводе есть упоминание о запущенных тестах
if echo "$TEST_OUTPUT" | grep -qi "0 tests ran\|no tests found\|discovered: 0"; then
    echo "ERROR: Tests did not run! Output shows 0 tests."
    exit 1
fi

# Проверяем, что есть упоминание о passed/failed (TUnit выводит статистику)
if ! echo "$TEST_OUTPUT" | grep -qi "passed\|failed\|skipped"; then
    echo "ERROR: Cannot determine test results. TUnit may be misconfigured."
    exit 1
fi

echo "OK: Tests were executed and results detected."
