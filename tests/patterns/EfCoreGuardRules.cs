// TRAP: Агент использует EF Core антипаттерны, которые рефлексия не видит
// или нарушает read/write path конвенции.
// GUARDRAIL: NetArchTest + starter regex checks ловят нарушения EF-специфичных правил.
// NOTE: Для стабильных C# semantic rules (FindAsync/Include/read-path) предпочитай Roslyn analyzer.
// Этот файл — только для проектов с EF Core. Для Dapper см. DapperGuardRules.cs.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(result.IsSuccessful).IsTrue()
// - xUnit:  [Fact] + Assert.True(result.IsSuccessful)
// - NUnit:  [Test] + Assert.That(result.IsSuccessful, Is.True)
// - MSTest: [TestMethod] + Assert.IsTrue(result.IsSuccessful)

using NetArchTest.Rules;
using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class EfCoreGuardRules
{
    // TRAP: Агент добавил FindAsync в query-handler, "потому что так короче".
    // GUARDRAIL: Regex-сканирование ловит FindAsync в read-path (QueryHandlers / QueryServices).
    // NOTE: Также ловится compile-time через BannedApiAnalyzers (RS0030) в BannedSymbols.txt.
    //       Regex тут — fallback / double-check.
    [Test]
    public void FindAsync_ShouldNotBeUsedInReadPath()
    {
        var violations = ScanSourceFiles(
            pattern: @"\.FindAsync\(",
            fileGlob: "*Query*.cs",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("FindAsync is only allowed in write-path / command handlers.");
    }

    // TRAP: Агент добавил DbContext в Application layer.
    // GUARDRAIL: Application знает только про Ports (интерфейсы).
    [Test]
    public void Application_ShouldNotReferenceEfCore()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Application.*")
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore")
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

    // --- Helpers: regex source scanning ---

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
            if (file.Contains("obj") || file.Contains("bin") || file.Contains("Tests"))
                continue;

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
}
