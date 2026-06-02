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
            .Because(FormatFailingTypes(result));
    }

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
            .Because(FormatFailingTypes(result));
    }

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
