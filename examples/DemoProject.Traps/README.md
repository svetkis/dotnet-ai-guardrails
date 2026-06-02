# DemoProject.Traps — Failing Demo for Skeptical AI Engineering

> **Intentionally broken code.** Каждый тест здесь падает, демонстрируя guardrail в действии.

## Запуск

```bash
cd examples/DemoProject.Traps
dotnet run --project tests/DemoProject.Traps.Tests
```

## Ловушки (TRAPs)

| Файл | Нарушение | Guardrail |
|------|-----------|-----------|
| `MutableState.cs` | `public int Counter;` в Domain | `BeImmutableExternally` |
| `DomainLeakingToInfra.cs` | `using System.Net.Http` в Domain | `NotHaveDependencyOnAny` |
| `PaymentService.cs` | `using Orders` из `Payments` | `NotHaveDependenciesBetweenSlices` |
| `RawGuidEntity.cs` | `Guid Id` вместо strongly typed ID | `NotHaveDependencyOnAny("System.Guid")` |

## Использование

1. Запустите тесты — увидите 4 падения с `IType.Explanation`.
2. "Почините" ловушку (уберите нарушение) — тест станет зелёным.
3. Используйте для onboarding команды: "вот что ловит guardrail, если агент сломает архитектуру".
