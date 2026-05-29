// TRAP: Агент использует FindAsync в read-path, нарушает слоистую архитектуру
// или добавляет антипаттерны, которые рефлексия не видит.
// GUARDRAIL: NetArchTest + regex-сканирование исходников ловят оба класса нарушений.

using NetArchTest.Rules;
using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class ArchitectureRules
{
    // TRAP: Агент добавил FindAsync в query-handler, "потому что так короче".
    // GUARDRAIL: FindAsync допустим только в Command-handlers (write-path).
    [Test]
    public void FindAsync_ShouldNotBeUsedInReadPath()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Queries.*")
            .Should().Not().HaveMethodNameMatching("FindAsync")
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент зареференсил Infrastructure из Api напрямую.
    // GUARDRAIL: Api → Application → Domain. Infrastructure только через DI.
    [Test]
    public void Api_ShouldNotReferenceInfrastructureDirectly()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Api.*")
            .Should().Not().DependOnAny(Types.That().ResideInNamespace(".*Infrastructure.*"))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент добавил DbContext в Application layer.
    // GUARDRAIL: Application знает только про Ports (интерфейсы).
    [Test]
    public void Application_ShouldNotReferenceEfCore()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Application.*")
            .Should().Not().DependOnAny(Types.That().HaveNameStartingWith("Microsoft.EntityFrameworkCore"))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент создал сервис без интерфейса в Application.
    // GUARDRAIL: Все сервисы должны иметь интерфейс (Port).
    [Test]
    public void Services_ShouldHaveInterfaces()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Infrastructure.*")
            .And().HaveNameEndingWith("Service")
            .Should().ImplementInterface(typeof(IService))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент использовал .Include() в QueryService — N+1 и лишние данные.
    // GUARDRAIL: Regex-сканирование ловит то, что рефлексия не видит.
    [Test]
    public void QueryServices_ShouldNotUse_Include()
    {
        var violations = ScanSourceFiles(
            pattern: @"\.Include\(",
            fileGlob: "*QueryService*.cs",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("QueryService must use .Select() projections, not .Include()");
    }

    // TRAP: Агент добавил кэширование без указания размера — OOM в проде.
    // GUARDRAIL: Каждый bare cache.Set() ловится сканированием.
    [Test]
    public void CacheSet_ShouldAlwaysSpecifySize()
    {
        var violations = ScanSourceFiles(
            pattern: @"(?<!Sized)\b_cache\.Set\(",
            fileGlob: "*.cs",
            whitelist: new[] { "CacheSetup.cs: explicit SizeLimit config" });

        Assert.That(violations).IsEmpty()
            .Because("MemoryCache SizeLimit requires every entry to specify .Size");
    }

    // TRAP: Агент создал дубликат ID для задокументированного решения.
    // GUARDRAIL: PERF-###, DB-###, AUD-### должны быть уникальны по всей кодбазе.
    [Test]
    public void PerfAndDbDecisions_ShouldHaveUniqueIds()
    {
        var ids = ExtractDecisionIds("src", @"(PERF|DB|AUD)-\d{3}");
        var duplicates = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

        Assert.That(duplicates).IsEmpty()
            .Because("Numbered decisions must be unique to prevent collision in documentation");
    }

    // --- Helper: regex source scanning ---

    private static IEnumerable<string> ScanSourceFiles(string pattern, string fileGlob, string[] whitelist)
    {
        var srcPath = Path.Combine("..", "..", "..", "..", "src");
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

                    // Simple whitelist check: if any whitelist entry contains the filename, skip
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
        var path = Path.Combine("..", "..", "..", "..", rootDir);
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
