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
            .HaveDependencyOn(ApplicationAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
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

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент использует FindAsync в read-path, нарушает слоистую архитектуру.
    // GUARDRAIL: Regex-сканирование исходников ловит антипаттерны, которые рефлексия не видит.
    [Test]
    public async Task SourceCode_ShouldNotUse_FindAsync_InQueryServices()
    {
        var violations = ScanSourceFiles(
            pattern: @"\.FindAsync\(",
            fileGlob: "*.cs",
            whitelist: Array.Empty<string>());

        await Assert.That(violations).IsEmpty()
            .Because("FindAsync is only allowed in write-path / command handlers.");
    }

    [Test]
    public async Task SourceCode_ShouldNotUse_Include_InQueryServices()
    {
        var violations = ScanSourceFiles(
            pattern: @"\.Include\(",
            fileGlob: "*QueryService*.cs",
            whitelist: Array.Empty<string>());

        await Assert.That(violations).IsEmpty()
            .Because("QueryService must use .Select() projections, not .Include()");
    }

    [Test]
    public async Task DecisionIds_ShouldBeUnique()
    {
        var ids = ExtractDecisionIds("src", @"(PERF|DB|AUD)-\d{3}");
        var duplicates = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

        await Assert.That(duplicates).IsEmpty()
            .Because("Numbered decisions must be unique to prevent collision in documentation.");
    }

    private static IEnumerable<string> ScanSourceFiles(string pattern, string fileGlob, string[] whitelist)
    {
        var srcPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src");
        srcPath = Path.GetFullPath(srcPath);

        if (!Directory.Exists(srcPath))
            return Array.Empty<string>();

        var files = Directory.GetFiles(srcPath, fileGlob, SearchOption.AllDirectories);
        var violations = new List<string>();
        var regex = new Regex(pattern);

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    var location = $"{relativePath}:{i + 1}";

                    if (whitelist.Any(w => location.Contains(w.Split(':')[0])))
                        continue;

                    violations.Add(location);
                }
            }
        }

        return violations;
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
