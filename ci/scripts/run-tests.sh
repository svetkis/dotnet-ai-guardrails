#!/bin/bash
# GUARDRAIL: Запускает все тестовые проекты через `dotnet run --project`.
# Автоматически находит тестовые проекты, чтобы не захардкоживать пути.
#
# Адаптация под проект:
# - Если тесты лежат в папке `tests/` — скрипт найдёт их сам.
# - Если тесты разбросаны по `src/` — измени `TEST_DIRS` ниже.

set -e

TEST_DIRS=("tests" "src")
FOUND=0

for dir in "${TEST_DIRS[@]}"; do
    if [ ! -d "$dir" ]; then
        continue
    fi

    # Ищем проекты с TUnit, xUnit, NUnit или MSTest
    while IFS= read -r -d '' proj; do
        FOUND=1
        echo "========================================"
        echo "Running tests: $proj"
        echo "========================================"
        dotnet run --project "$proj" --configuration Release
    done < <(find "$dir" -name "*.csproj" -print0 | while IFS= read -r -d '' proj; do
        if grep -qiE "TUnit|xUnit|NUnit|MSTest|Microsoft\.NET\.Test\.Sdk" "$proj"; then
            printf '%s\0' "$proj"
        fi
    done)
done

if [ "$FOUND" -eq 0 ]; then
    echo "ERROR: No test projects found in ${TEST_DIRS[*]}."
    echo "Adapt TEST_DIRS in ci/scripts/run-tests.sh to match your structure."
    exit 1
fi
