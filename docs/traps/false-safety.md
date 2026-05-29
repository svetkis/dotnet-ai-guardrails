# Ловушка: Ложная безопасность (False Safety)

## Сценарий

Агент обновляет TUnit или меняет `.csproj`. В результате `dotnet test` молча выдаёт:

```
Build succeeded.
Test run finished: 0 tests ran
```

Exit code: 0. CI зелёный. Код мержится.

## Почему это опасно

Две недели команда думает, что всё проверено. На самом деле:
- Новый баг не пойман
- Регрессия прошла
- Агент сломал настройки runner'а

## Root Causes

- TUnit + .NET 10 + MTP: `dotnet test` не всегда корректно запускает TUnit
- Агент удалил `<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>`
- Агент переименовал test project, но CI всё ещё указывает на старый путь

## Решение

1. **`dotnet run --project`** вместо `dotnet test`
2. **Verify script** — `ci/scripts/verify-tests.sh` парсит вывод и проверяет, что count > 0
3. **CI guardrail** — отдельный шаг, который падает если "0 tests ran"

## Паттерн

См. `tests/conventions/TUnit_Guide.md` и `ci/github-actions/safe-ci.yml`
