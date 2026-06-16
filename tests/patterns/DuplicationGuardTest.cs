// TRAP: Агент скопировал важную бизнес-логику в новый сервис вместо реюса.
// GUARDRAIL: Starter regex check ловит буквальное дублирование критичных бизнес-фрагментов.
// LIMIT: Ловит только буквальное (literal) дублирование. Семантическое дублирование
// (order.IsConfirmed() vs order.Status == Confirmed) — это задача code-review чеклиста техлида.
// См. templates/skills/code-review/CHECKLIST.md → "Дублирование бизнес-логики (Semantic)".

using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class DuplicationGuardTest
{
    // TRAP: Агент добавил проверку статуса заказа в новый сервис, хотя она уже есть в Domain.
    // GUARDRAIL: Паттерн бизнес-правила встречается только в одном production-файле.
    [Test]
    public void BusinessRule_ShouldNotBeDuplicatedAcrossServices()
    {
        // Настрой: список regex-паттернов критичной бизнес-логики, которые должны быть уникальны
        var businessPatterns = new[]
        {
            @"Status\s*==\s*BookingStatus\.Confirmed", // Пример: проверка статуса
            @"Total\s*\*\s*0\.\d+",                    // Пример: расчёт скидки
            @"DateTime\.Now",                           // Anti-pattern: должен быть UtcNow или IClock
        };

        var srcPath = Path.Combine("..", "..", "..", "..", "src");
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

        Assert.That(violations).IsEmpty()
            .Because("Critical business logic must live in one place (Domain or shared service)");
    }

    // TRAP: Агент захардкодил магическую строку/число в нескольких местах.
    // GUARDRAIL: Константы домена должны быть объявлены один раз.
    [Test]
    public void MagicValues_ShouldBeCentralized()
    {
        // Настрой: магические значения, которые не должны размазываться
        var magicPatterns = new[]
        {
            @"\"Bearer \"",          // Должно быть в константе AuthScheme
            @"MaxItems\s*=\s*50",    // Должно быть в доменной константе
        };

        var srcPath = Path.Combine("..", "..", "..", "..", "src");
        if (!Directory.Exists(srcPath))
            Assert.Fail("Source directory not found");

        var violations = new List<string>();

        foreach (var pattern in magicPatterns)
        {
            var matches = ScanFiles(srcPath, pattern);
            var productionFiles = matches
                .Where(m => !m.Contains("Tests"))
                .Select(m => m.Split(':')[0])
                .Distinct()
                .ToList();

            if (productionFiles.Count > 1)
            {
                violations.Add($"Magic value '{pattern}' found in {productionFiles.Count} files: {string.Join(", ", productionFiles)}");
            }
        }

        Assert.That(violations).IsEmpty()
            .Because("Magic values must be declared as constants in Domain/Constants");
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
