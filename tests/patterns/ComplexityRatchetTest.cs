// TRAP: Агент добавляет ветвления и вложенность в метод, превращая его в нечитаемый клубок.
// GUARDRAIL: Количество методов с нарушениями S3776 (cognitive) / S1541 (cyclomatic) не растёт.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Этот паттерн предпочитает SonarAnalyzer.CSharp (S3776/S1541) как источник сложности.
//       Альтернатива — Microsoft.CodeAnalysis.Metrics, но она медленнее и требует доп. пакетов.

using System.Diagnostics;
using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class ComplexityRatchetTests
{
    // TRAP: Агент написал метод с 5 уровнями вложенности if/switch/foreach.
    // GUARDRAIL: Фиксируем baseline нарушений и не даём ему расти.
    [Test]
    public void SonarComplexityViolations_ShouldNotIncrease()
    {
        var violationCount = CountSonarComplexityViolations("..");
        var baseline = GetBaselineOrSet(violationCount, "complexity-baseline.txt");

        Assert.That(violationCount)
            .IsLessThanOrEqualTo(baseline)
            .Because($"Cognitive/cyclomatic complexity violations must not increase. " +
                     $"Current={violationCount}, Baseline={baseline}. Refactor the new method or update the baseline consciously.");
    }

    // TRAP: В legacy-проекте нарушений слишком много, и ratchet не работает.
    // GUARDRAIL: Проверяем, что worst-методы не стали сложнее (топ-10 hotspots ratchet).
    [Test]
    public void TopHotspotComplexity_ShouldNotIncrease()
    {
        var hotspots = GetTopComplexityHotspots("..", topN: 10);
        var currentMax = hotspots.Any() ? hotspots.Max(h => h.Complexity) : 0;
        var baselineMax = GetBaselineOrSet(currentMax, "complexity-hotspot-baseline.txt");

        Assert.That(currentMax)
            .IsLessThanOrEqualTo(baselineMax)
            .Because("Max complexity of top hotspots must not increase. " +
                     "CurrentMax={0}, BaselineMax={1}", currentMax, baselineMax);
    }

    // --- Helpers ---

    private static int CountSonarComplexityViolations(string rootDir)
    {
        // Build with SonarAnalyzer and capture S3776/S1541 warnings.
        // Requires SonarAnalyzer.CSharp package and TreatWarningsAsErrors=false for this scan.
        var solutionFile = Directory.GetFiles(rootDir, "*.sln", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(rootDir, "*.slnx", SearchOption.TopDirectoryOnly))
            .FirstOrDefault();

        if (solutionFile == null)
            return 0;

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{solutionFile}\" -warnaserror:false -clp:NoSummary",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = rootDir
        };

        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // Match S3776 (cognitive) and S1541 (cyclomatic) warnings.
        var matches = Regex.Matches(output, @"(S3776|S1541)");
        return matches.Count;
    }

    private static IReadOnlyList<Hotspot> GetTopComplexityHotspots(string rootDir, int topN)
    {
        // Placeholder: in a real project, parse SonarLint/ReportGenerator output
        // or use Microsoft.CodeAnalysis.Metrics to compute per-method complexity.
        // For copy-paste friendly template, return empty list; replace with real scanner.
        return Array.Empty<Hotspot>();
    }

    private static int GetBaselineOrSet(int current, string baselineFile)
    {
        var path = Path.Combine("..", baselineFile);
        if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out var baseline))
            return baseline;

        File.WriteAllText(path, current.ToString());
        return current;
    }

    private readonly record struct Hotspot(string MethodName, int Complexity);
}
