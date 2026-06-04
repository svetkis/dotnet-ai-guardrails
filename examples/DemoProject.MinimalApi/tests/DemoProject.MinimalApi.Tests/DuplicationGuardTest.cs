// TRAP: Агент скопировал важную бизнес-логику в новый сервис вместо реюза.
// GUARDRAIL: Regex-сканирование ловит дублирование критичных бизнес-фрагментов в разных файлах.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(violations).IsEmpty()
// - xUnit:  [Fact] + Assert.Empty(violations)
// - NUnit:  [Test] + Assert.That(violations, Is.Empty)

using System.Text.RegularExpressions;
using TUnit;

namespace DemoProject.MinimalApi.Tests;

public class DuplicationGuardTest
{
    [Test]
    public async Task BusinessRule_ShouldNotBeDuplicatedAcrossServices()
    {
        // ADAPT: замените паттерны на реальные бизнес-правила вашего проекта.
        // В MinimalApi-примере валидаций нет — поэтому список пуст.
        var businessPatterns = Array.Empty<string>();

        var srcPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src");
        srcPath = Path.GetFullPath(srcPath);

        if (!Directory.Exists(srcPath))
            Assert.Fail("Source directory not found");

        var violations = new List<string>();

        foreach (var pattern in businessPatterns)
        {
            var matches = ScanFiles(srcPath, pattern);
            var productionFiles = matches
                .Where(m => !m.Contains("Tests"))
                .Select(m => m.Split(':')[0])
                .Distinct()
                .ToList();

            if (productionFiles.Count > 1)
            {
                violations.Add($"Pattern '{pattern}' duplicated in {productionFiles.Count} files: {string.Join(", ", productionFiles)}");
            }
        }

        await Assert.That(violations).IsEmpty()
            .Because("Critical business logic must live in one place (Domain or shared service)");
    }

    private static List<string> ScanFiles(string rootDir, string pattern)
    {
        var files = Directory.GetFiles(rootDir, "*.cs", SearchOption.AllDirectories);
        var regex = new Regex(pattern);
        var matches = new List<string>();

        foreach (var file in files)
        {
            if (file.Contains("obj") || file.Contains("bin"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    matches.Add($"{relative}:{i + 1}");
                }
            }
        }

        return matches;
    }
}
