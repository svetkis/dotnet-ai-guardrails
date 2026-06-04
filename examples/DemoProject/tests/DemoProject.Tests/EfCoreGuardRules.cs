// TRAP: Агент использует EF Core антипаттерны, которые рефлексия не видит
// или нарушает read/write path конвенции.
// GUARDRAIL: Regex-сканирование исходников ловит нарушения EF-специфичных правил.
// NOTE: Этот файл — только для проектов с EF Core. Для Dapper см. DapperGuardRules.cs.

using System.Reflection;
using System.Text.RegularExpressions;
using DemoProject.Domain;
using NetArchTest.Rules;
using TUnit;

namespace DemoProject.Tests;

public class EfCoreGuardRules
{
    private static readonly Assembly ApplicationAssembly = typeof(Application.BookingService).Assembly;

    // TRAP: Агент добавил DbContext в Application layer.
    // GUARDRAIL: Application знает только про Ports (интерфейсы).
    [Test]
    public async Task Application_ShouldNotReferenceEfCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore")
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because(FormatFailingTypes(result));
    }

    // TRAP: Агент использует FindAsync в read-path, нарушает слоистую архитектуру.
    // GUARDRAIL: Regex-сканирование исходников ловит антипаттерны, которые рефлексия не видит.
    // NOTE: Также ловится compile-time через BannedApiAnalyzers (RS0030) в BannedSymbols.txt.
    //       Regex тут — fallback / double-check для случаев, когда analyzer не подхватился.
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

    // TRAP: Агент использовал .Include() в QueryService — N+1 и лишние данные.
    // GUARDRAIL: Regex-сканирование исходников ловит то, что рефлексия не видит.
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

    private static string FormatFailingTypes(NetArchTest.Rules.TestResult result)
    {
        if (result.IsSuccessful)
            return string.Empty;

        var lines = result.FailingTypes
            .Select(t => $"- {t.FullName}: {t.Explanation}")
            .ToList();

        return "Failing types:\n" + string.Join("\n", lines);
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
}
