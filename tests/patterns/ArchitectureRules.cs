// TRAP: Агент нарушает слоистую архитектуру, добавляет антипаттерны или создаёт дубли ID решений.
// GUARDRAIL: NetArchTest ловит архитектурные зависимости.
// NOTE: Regex-проверки ниже — starter/fallback для артефактов и временных C# spikes.
// Для стабильных C# semantic rules предпочитай Roslyn analyzer (Layer 1.1).
// NOTE: Для EF Core-специфичных правил см. EfCoreGuardRules.cs.
//       Для Dapper-специфичных правил см. DapperGuardRules.cs.
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

public class ArchitectureRules
{
    // TRAP: Агент зареференсил Infrastructure из Api напрямую.
    // GUARDRAIL: Api → Application → Domain. Infrastructure только через DI.
    [Test]
    public void Api_ShouldNotReferenceInfrastructureDirectly()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Api.*")
            .Should().NotHaveDependencyOnAny(".*Infrastructure.*")
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

    // TRAP: Агент добавил mutable state в Domain через public field/setter.
    // GUARDRAIL: BeImmutableExternally ловит mutable public API (eNhancedEdition 1.4.5+).
    // NOTE: Авто-свойства (auto-properties) могут не детектироваться — используйте Roslyn analyzers для точной проверки.
    [Test]
    public void DomainTypes_ShouldBeImmutableExternally()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Domain.*")
            .And().AreNotEnums()
            .Should().BeImmutableExternally()
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент добавил кэширование без указания размера — OOM в проде.
    // GUARDRAIL: Каждый bare cache.Set() ловится сканированием.
    // NOTE: Универсальное правило, не зависит от ORM.
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
            .Because("Decision guards must be unique to prevent collision in documentation");
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
