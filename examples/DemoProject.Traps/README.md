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
| `Modules/` (Orders→Payments→Shipping→Orders) | Циклические зависимости между модулями | `ArchUnitNET.BeFreeOfCycles` |
| `RawGuidEntity.cs` | `Guid Id` вместо strongly typed ID | `NotHaveDependencyOnAny("System.Guid")` |
| `AllocationBudgetHotspot.cs` | `new List<int>` в методе с `[HotPath]` | `AllocationBudgetTests` (baseline + 10%) |

## Использование

1. Запустите тесты — увидите 6 падений (5 с `IType.Explanation`, 1 с ArchUnitNET).
2. "Почините" ловушку (уберите нарушение) — тест станет зелёным.
3. Используйте для onboarding команды: "вот что ловит guardrail, если агент сломает архитектуру".
