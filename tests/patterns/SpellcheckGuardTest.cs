// TRAP: Опечатка в имени public property/DTO/endpoint утекает в API-контракт и становится обратно несовместимой.
// GUARDRAIL: CSpell проверяет markdown, комментарии и публичные символы; новых опечаток быть не должно.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Требуется установленный глобально или локально `cspell`:
//       npm install -g cspell
//       или dotnet tool install --global cspell

using System.Diagnostics;
using TUnit;

namespace Tests.Patterns;

public class SpellcheckGuardTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    // TRAP: Агент добавил опечатку в публичное API-имя.
    // GUARDRAIL: CSpell не находит новых ошибок в проверяемых файлах.
    [Test]
    public void CSpell_ShouldNotFindNewMisspellings()
    {
        var misspellingCount = RunCSpell(RepoRoot);
        var baseline = GetBaselineOrSet(misspellingCount, "spellcheck-baseline.txt");

        Assert.That(misspellingCount)
            .IsLessThanOrEqualTo(baseline)
            .Because("Spellcheck violations must not increase. " +
                     "Current={0}, Baseline={1}. Add exceptions to project dictionary if needed.",
                     misspellingCount, baseline);
    }

    // --- Helpers ---

    private static int RunCSpell(string rootDir)
    {
        var configPath = Path.Combine(rootDir, "cspell.json");
        if (!File.Exists(configPath))
        {
            // No config — spellcheck is not enabled yet. Return 0 to avoid blocking.
            return 0;
        }

        var psi = new ProcessStartInfo
        {
            FileName = "cspell",
            Arguments = $"lint --no-progress --unique \"{rootDir}/**/*.{GetSupportedExtensions()}\"",
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

            // cspell exit code 1 = issues found. Count "Unknown word" lines.
            return output.Split('\n').Count(line => line.Contains("Unknown word"));
        }
        catch (Exception ex)
        {
            Assert.Fail($"cspell is not installed or failed: {ex.Message}");
            return int.MaxValue;
        }
    }

    private static string GetSupportedExtensions() => "{md,cs,ts,tsx,json,yml,yaml}";

    private static int GetBaselineOrSet(int current, string baselineFile)
    {
        var path = Path.Combine(RepoRoot, baselineFile);
        if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out var baseline))
            return baseline;

        File.WriteAllText(path, current.ToString());
        return current;
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, ".git"))
                || Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly).Any()
                || Directory.GetFiles(dir, "*.slnx", SearchOption.TopDirectoryOnly).Any())
            {
                return dir;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not find repository root. Ensure .git, .sln or .slnx exists.");
    }
}
