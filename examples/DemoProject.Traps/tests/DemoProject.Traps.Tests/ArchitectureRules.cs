// GUARDRAIL: NetArchTest.eNhancedEdition ловит архитектурные ловушки AI-агента.
// Этот проект — failing demo: каждый тест здесь ДОЛЖЕН падать,
// потому что в src/DemoProject.Traps специально созданы нарушения.

using System.Reflection;
using NetArchTest.Rules;
using TUnit;

namespace DemoProject.Traps.Tests;

public class ArchitectureRules
{
    private static readonly Assembly TrapsAssembly = typeof(Domain.MutableState).Assembly;

    // TRAP: Агент добавил mutable state в Domain через public field.
    // GUARDRAIL: BeImmutableExternally ловит public fields / mutable surface.
    // NOTE: NetArchTest может не заполнять Explanation для этого правила.
    //       Поэтому добавляем human-readable message в Because.
    [Test]
    public async Task DomainTypes_ShouldBeImmutableExternally()
    {
        var result = Types.InAssembly(TrapsAssembly)
            .That().ResideInNamespace("DemoProject.Traps.Domain")
            .And().AreNotEnums()
            .Should()
            .BeImmutableExternally()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(result.IsSuccessful
                ? string.Empty
                : "Domain types must be immutable externally. " +
                  "Check for public fields, public setters, or mutable collections. " +
                  "Failing types: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName)));
    }

    // TRAP: Агент добавил using System.Net.Http в Domain для "одного вызова".
    // GUARDRAIL: HaveDependencyOnAny ловит IL-зависимость от запрещённого namespace.
    [Test]
    public async Task Domain_ShouldNotDependOn_SystemNetHttp()
    {
        var result = Types.InAssembly(TrapsAssembly)
            .That().ResideInNamespace("DemoProject.Traps.Domain")
            .Should()
            .NotHaveDependencyOnAny("System.Net.Http")
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    // TRAP: Агент добавил using из соседней фичи "ради одного DTO".
    // GUARDRAIL: Slice().NotHaveDependenciesBetweenSlices() ловит межмодульную зависимость.
    // NOTE: NetArchTest может не заполнять Explanation для slice-правил.
    [Test]
    public async Task Features_ShouldNotDependOn_EachOther()
    {
        var result = Types.InAssembly(TrapsAssembly)
            .Slice()
            .ByNamespacePrefix("DemoProject.Traps.Features")
            .Should()
            .NotHaveDependenciesBetweenSlices()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(result.IsSuccessful
                ? string.Empty
                : "Features must not depend on each other. " +
                  "Each feature slice should be self-contained. " +
                  "Failing types: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName)));
    }

    // TRAP: Агент использовал Guid вместо strongly typed ID.
    // GUARDRAIL: Regex + архитектурные тесты ловят сырые Guid в именах свойств.
    [Test]
    public async Task Entities_ShouldNotUseRawGuidForIds()
    {
        var result = Types.InAssembly(TrapsAssembly)
            .That().ResideInNamespace("DemoProject.Traps.Domain")
            .And().HaveNameEndingWith("Entity")
            .Should()
            .NotHaveDependencyOnAny("System.Guid")
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    private static string FormatFailingTypes(NetArchTest.Rules.TestResult result)
    {
        if (result.IsSuccessful)
            return string.Empty;

        var lines = result.FailingTypes
            .Select(t => $"- {t.FullName}: {t.Explanation}")
            .ToList();

        return "Failing types:\n" + string.Join("\n", lines);
    }
}
