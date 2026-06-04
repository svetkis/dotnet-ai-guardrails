// GUARDRAIL: NetArchTest + regex-сканирование исходников ловят нарушения архитектуры.
// Этот файл — рабочая адаптация шаблона из tests/patterns/ArchitectureRules.cs

using System.Reflection;
using System.Text.RegularExpressions;
using DemoProject.Domain;
using NetArchTest.Rules;
using TUnit;

namespace DemoProject.Tests;

public class ArchitectureRules
{
    private static readonly Assembly DomainAssembly = typeof(Booking).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.BookingService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.InfrastructureBookingService).Assembly;

    [Test]
    public async Task Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    [Test]
    public async Task Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    [Test]
    public async Task Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    [Test]
    public async Task Services_ShouldHaveInterfaces()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .Should()
            .ImplementInterface(typeof(IBookingService))
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    // TRAP: Агент добавил public set в доменный тип, нарушая иммутабельность value object.
    // GUARDRAIL: AreImmutableExternally ловит mutable public API в Domain-слое.
    // NOTE: Для чистой иммутабельности используй BeImmutable(); здесь — external surface.
    [Test]
    public async Task DomainTypes_ShouldBeImmutableExternally()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace("DemoProject.Domain")
            .And().AreNotEnums()
            .Should()
            .BeImmutableExternally()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    // TRAP: Агент создал дубликат ID для задокументированного решения.
    // GUARDRAIL: PERF-###, DB-###, AUD-### должны быть уникальны по всей кодбазе.
    [Test]
    public async Task DecisionIds_ShouldBeUnique()
    {
        var ids = ExtractDecisionIds("src", @"(PERF|DB|AUD)-\d{3}");
        var duplicates = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

        await Assert.That(duplicates).IsEmpty()
            .Because("Decision guards must be unique to prevent collision in documentation.");
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

    private static IEnumerable<string> ExtractDecisionIds(string rootDir, string regexPattern)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", rootDir);
        path = Path.GetFullPath(path);

        if (!Directory.Exists(path))
            return Array.Empty<string>();

        var regex = new Regex(regexPattern);
        var ids = new List<string>();

        foreach (var file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in regex.Matches(text))
                ids.Add(match.Value);
        }

        return ids;
    }
}
