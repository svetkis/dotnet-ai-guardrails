// TRAP: Тесты проходят, но не проверяют логику — мутанты выживают, а баги просачиваются в прод.
// GUARDRAIL: Stryker.NET запускается перед релизом; mutation score не падает.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Stryker.NET на момент 2026-06 не поддерживает TUnit / Microsoft Testing Platform.
//       Используйте этот паттерн как periodic audit или CI job через dotnet test проекта.

using System.Diagnostics;
using System.Text.Json;
using TUnit;

namespace Tests.Patterns;

public class MutationGuardTests
{
    // TRAP: Покрытие строк высокое, но ассерты слабые — мутанты выживают.
    // GUARDRAIL: Mutation score для критичной сборки >= baseline (например, 70%).
    [Test]
    public void StrykerMutationScore_ShouldMeetBaseline()
    {
        var score = RunStrykerAndGetScore("..", targetProject: "src/YourProject.Domain/YourProject.Domain.csproj");
        var baseline = GetBaselineOrSet((int)(score * 100), "mutation-score-baseline.txt");
        var current = (int)(score * 100);

        Assert.That(current)
            .IsGreaterThanOrEqualTo(baseline)
            .Because("Mutation score must not decrease. Current={0}%, Baseline={1}%", current, baseline);
    }

    // --- Helpers ---

    private static double RunStrykerAndGetScore(string rootDir, string targetProject)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"stryker --project {targetProject} --break-at 0",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = rootDir
        };

        try
        {
            using var process = Process.Start(psi)!;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse mutation score from Stryker output or report.
            // In real setup, read StrykerOutput/{timestamp}/reports/mutation-report.json
            var reportPath = Directory.GetFiles(Path.Combine(rootDir, "StrykerOutput"), "mutation-report.json", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTime)
                .FirstOrDefault();

            if (reportPath == null)
                return 0;

            var json = File.ReadAllText(reportPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("mutationScore", out var scoreProp) && scoreProp.TryGetDouble(out var score))
                return score / 100.0;

            return 0;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Stryker is not installed or failed: {ex.Message}");
            return 0;
        }
    }

    private static int GetBaselineOrSet(int current, string baselineFile)
    {
        var path = Path.Combine("..", baselineFile);
        if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out var baseline))
            return baseline;

        File.WriteAllText(path, current.ToString());
        return current;
    }
}
