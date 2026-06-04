// TRAP: Агент пишет непараметризованный SQL, использует string interpolation в запросах
// или забывает таймауты при Dapper-вызовах.
// GUARDRAIL: Regex-сканирование исходников ловит SQL-инъекции и антипаттерны Dapper.
// NOTE: Этот файл — только для проектов с Dapper / Raw SQL. Для EF Core см. EfCoreGuardRules.cs.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(violations).IsEmpty()
// - xUnit:  [Fact] + Assert.Empty(violations)
// - NUnit:  [Test] + Assert.That(violations, Is.Empty)
// - MSTest: [TestMethod] + Assert.AreEqual(0, violations.Count())

using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class DapperGuardRules
{
    // TRAP: Агент использовал C# string interpolation ($"...") в SQL-запросе.
    // GUARDRAIL: Любая строковая интерполяция в SQL — потенциальная инъекция.
    [Test]
    public void RawSql_ShouldNotUseStringInterpolation()
    {
        var violations = ScanSourceFiles(
            pattern: @"\$""[^""]*\{[^}]+\}[^""]*""",
            fileGlob: "*.cs",
            whitelist: new[] { "Migration", "SeedData", "Comment" });

        Assert.That(violations).IsEmpty()
            .Because("SQL queries must use parameterized statements (@param), never C# string interpolation");
    }

    // TRAP: Агент сконкатенировал user input в SQL-строку.
    // GUARDRAIL: Конкатенация строк с SQL-ключевыми словами — запрещена.
    [Test]
    public void RawSql_ShouldNotUseStringConcatenation()
    {
        var violations = ScanSourceFiles(
            pattern: @"(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|JOIN)\s*[^""]*\+\s*",
            fileGlob: "*.cs",
            whitelist: new[] { "Migration", "SeedData" });

        Assert.That(violations).IsEmpty()
            .Because("SQL must be static or parameterized. Concatenation enables injection.");
    }

    // TRAP: Агент вызвал QueryAsync / ExecuteAsync без commandTimeout — риск вечного ожидания.
    // GUARDRAIL: Каждый Dapper-вызов должен явно передавать timeout или использовать глобальный default.
    [Test]
    public void DapperCalls_ShouldHaveCommandTimeout()
    {
        // Ищем вызовы Dapper без третьего аргумента commandTimeout
        // Примеры: connection.QueryAsync<Order>(sql, param) — нарушение
        //          connection.QueryAsync<Order>(sql, param, commandTimeout: 30) — ок
        var violations = ScanSourceFiles(
            pattern: @"\.(QueryAsync|ExecuteAsync|QueryFirstAsync|QuerySingleAsync)<.*?>\s*\([^,]+,[^,]+\)",
            fileGlob: "*.cs",
            whitelist: new[] { "GlobalCommandTimeout.cs: default timeout configured" });

        Assert.That(violations).IsEmpty()
            .Because("Dapper calls must specify commandTimeout to prevent hanging queries");
    }

    // TRAP: Агент построил динамический IN-клауз через string.Join без whitelist.
    // GUARDRAIL: IN с динамическим списком — только через TVP или ORM-генерацию.
    [Test]
    public void DynamicInClause_ShouldBeParameterized()
    {
        var violations = ScanSourceFiles(
            pattern: @"string\.Join\s*\(\s*"",""\s*,\s*\w+\s*\).*?(IN|in)\s*\(",
            fileGlob: "*.cs",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("Dynamic IN clauses must use Table-Valued Parameters (TVP) or parameterized ORM, not string.Join");
    }

    // TRAP: Агент использовал FromSqlRaw / ExecuteSqlRaw с интерполяцией в EF-проекте.
    // GUARDRAIL: Даже в EF raw SQL должен быть параметризован (FromSqlInterpolated).
    // NOTE: Это пересечение EF + raw SQL — ставим в Dapper-файл, т.к. относится к raw SQL hygiene.
    [Test]
    public void EfRawSql_ShouldNotUseInterpolation()
    {
        var violations = ScanSourceFiles(
            pattern: @"FromSqlRaw\s*\(\s*\$",
            fileGlob: "*.cs",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("Use FromSqlInterpolated for parameterized raw SQL. FromSqlRaw with $ is injection-prone.");
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
            // Skip generated and test files
            if (file.Contains("obj") || file.Contains("bin") || file.Contains("Tests"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    var location = $"{relativePath}:{i + 1}";

                    // Simple whitelist check
                    if (whitelist.Any(w => location.Contains(w.Split(':')[0])))
                        continue;

                    violations.Add(location);
                }
            }
        }

        return violations;
    }
}
