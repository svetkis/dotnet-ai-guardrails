// GUARDRAIL: ArchUnitNET ловит циклические зависимости между слайсами,
// которые NetArchTest.NotHaveDependenciesBetweenSlices не различает.
// TRAP: Агент добавляет межмодульные вызовы через mediator / events / shared kernel,
// создавая цикл Orders -> Payments -> Shipping -> Orders.
// NetArchTest запрещает ЛЮБЫЕ зависимости между слайсами (zero-tolerance).
// ArchUnitNET позволяет иметь DAG (направленный ациклический граф),
// но ловит только циклы.

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using TUnit;

namespace Tests.Patterns;

public class ArchUnitNetSliceTests
{
    // TIP: загружай архитектуру один раз в static readonly для производительности.
    // ArchUnitNET читает байткод через Mono.Cecil — это дороже, чем NetArchTest.
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(ArchUnitNetSliceTests).Assembly)
        .Build();

    [Test]
    public async Task Modules_ShouldBeFreeOfCycles()
    {
        // GUARDRAIL: Циклические зависимости между модулями/фичами.
        // Разрешаем DAG: Orders -> Payments -> Shipping.
        // Запрещаем цикл: Shipping -> Orders.
        IArchRule rule = SliceRuleDefinition.Slices()
            .Matching("MyApp.Modules.(*)..")
            .Should()
            .BeFreeOfCycles();

        await Assert.That(rule.HasNoViolations(Architecture)).IsTrue();
    }

    [Test]
    public async Task Modules_ShouldNotDependOnEachOther()
    {
        // GUARDRAIL: Альтернатива NetArchTest.NotHaveDependenciesBetweenSlices.
        // Zero-tolerance: любая зависимость между слайсами запрещена.
        // Используй, когда модули должны быть полностью изолированы.
        // NOTE: Это тот же guardrail, что и NetArchTest, но через ArchUnitNET API.
        IArchRule rule = SliceRuleDefinition.Slices()
            .Matching("MyApp.Modules.(*)..")
            .Should()
            .NotDependOnEachOther();

        await Assert.That(rule.HasNoViolations(Architecture)).IsTrue();
    }
}
