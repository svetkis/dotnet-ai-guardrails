// GUARDRAIL: Regex-сканирование исходников ловит дублирование критичных бизнес-фрагментов.
// Этот файл — рабочая адаптация шаблона из tests/patterns/DuplicationGuardTest.cs

using System.Text.RegularExpressions;
using TUnit;

namespace DemoProject.Tests;

public class DuplicationGuardTest
{
    // TRAP: Агент добавил проверку статуса бронирования в новый сервис вместо реюса.
    // GUARDRAIL: Паттерн бизнес-правила встречается только в одном production-файле.
    [Test]
    public async Task BusinessRule_ShouldNotBeDuplicatedAcrossServices()
    {
        var businessPatterns = new[]
        {
            @"Status\s*==\s*BookingStatus\.Confirmed",
            @"DateTime\.Now",
        };

        var violations = new List<string>();

        foreach (var pattern in businessPatterns)
        {
            var matches = ScanFiles(pattern);
            var productionFiles = matches
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

    private static List<string> ScanFiles(string pattern)
    {
        var srcPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src");
        srcPath = Path.GetFullPath(srcPath);

        if (!Directory.Exists(srcPath))
            return new List<string>();

        var files = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);
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
