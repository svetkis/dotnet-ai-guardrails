// TRAP: Агент использует preview SDK или устаревшие NuGet-пакеты,
// опираясь на training data вместо актуального состояния экосистемы.
// GUARDRAIL: Regex-сканирование global.json, *.csproj, package.json
// ловит preview-флаги и рассогласование версий.

using System.Text.RegularExpressions;
using TUnit;

namespace Tests.Patterns;

public class VersionAuditTest
{
    // TRAP: Агент притащил .NET 10 preview, хотя в команде стандарт — stable SDK.
    // GUARDRAIL: global.json не должен содержать preview/rc/beta в version.
    [Test]
    public void GlobalJson_ShouldNotReferencePreviewSdk()
    {
        var violations = ScanFilesForPattern(
            rootDir: ".",
            fileGlob: "global.json",
            pattern: @"""version""\s*:\s*""[^""]*(?:preview|rc|beta)[^""]*""",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("Stable SDK only. Preview versions require explicit whitelist entry");
    }

    // TRAP: Агент использует EF Core 8 в проекте на .NET 9.
    // GUARDRAIL: PackageReference Microsoft.* должен совпадать с TargetFramework.
    [Test]
    public void MicrosoftPackages_ShouldMatchTargetFramework()
    {
        var targetFramework = ExtractTargetFramework(".");
        var majorVersion = ExtractMajorVersion(targetFramework);

        var violations = new List<string>();
        var csprojFiles = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);

        foreach (var file in csprojFiles)
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                // Match Microsoft.* packages with explicit version
                var match = Regex.Match(line, @"<PackageReference\s+Include=""(Microsoft\.[^""]+)""\s+Version=""(\d+)\.");
                if (match.Success)
                {
                    var packageMajor = int.Parse(match.Groups[2].Value);
                    if (packageMajor != majorVersion)
                    {
                        violations.Add($"{file}:{i + 1} -> {match.Groups[0].Value} (expected major={majorVersion})");
                    }
                }
            }
        }

        Assert.That(violations).IsEmpty()
            .Because($"Microsoft.* packages major version must match TargetFramework major ({majorVersion})");
    }

    // TRAP: Агент добавил preview-версию NuGet-пакета.
    // GUARDRAIL: PackageReference не должен содержать preview/rc/beta.
    [Test]
    public void PackageReferences_ShouldNotBePrerelease()
    {
        var violations = ScanFilesForPattern(
            rootDir: ".",
            fileGlob: "*.csproj",
            pattern: @"<PackageReference[^>]*Version=""[^""]*(?:preview|rc|beta|alpha)[^""]*""",
            whitelist: new[] { "DemoProject.Tests.csproj: explicitly testing preview features" });

        Assert.That(violations).IsEmpty()
            .Because("Pre-release packages require explicit whitelist entry with justification");
    }

    // TRAP: Агент оставил frontend-зависимость на alpha/beta.
    // GUARDRAIL: package.json dependencies не содержат prerelease.
    [Test]
    public void PackageJson_ShouldNotContainPrereleaseDependencies()
    {
        var violations = new List<string>();
        var packageJsonFiles = Directory.GetFiles(".", "package.json", SearchOption.AllDirectories);

        foreach (var file in packageJsonFiles)
        {
            var text = File.ReadAllText(file);
            var matches = Regex.Matches(text, @"""[^""]+""\s*:\s*""[^""]*(?:alpha|beta|rc|preview)[^""]*""");
            foreach (Match match in matches)
            {
                violations.Add($"{file}: {match.Value}");
            }
        }

        Assert.That(violations).IsEmpty()
            .Because("Frontend dependencies must be stable releases");
    }

    // TRAP: Агент использует устаревший actions/checkout@v2/v3 в CI.
    // GUARDRAIL: GitHub Actions используют современные major versions.
    [Test]
    public void GitHubActions_ShouldNotUseLegacyActionVersions()
    {
        var violations = ScanFilesForPattern(
            rootDir: ".github",
            fileGlob: "*.yml",
            pattern: @"uses:\s+actions/(checkout|setup-dotnet)@v[123]\b",
            whitelist: Array.Empty<string>());

        Assert.That(violations).IsEmpty()
            .Because("GitHub Actions must use v4+ for checkout and setup-dotnet");
    }

    // --- Helpers ---

    private static IEnumerable<string> ScanFilesForPattern(
        string rootDir, string fileGlob, string pattern, string[] whitelist)
    {
        var violations = new List<string>();
        if (!Directory.Exists(rootDir))
            return violations;

        var files = Directory.GetFiles(rootDir, fileGlob, SearchOption.AllDirectories);
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    var location = $"{file}:{i + 1}";
                    if (whitelist.Any(w => location.Contains(w.Split(':')[0])))
                        continue;

                    violations.Add(location);
                }
            }
        }

        return violations;
    }

    private static string ExtractTargetFramework(string rootDir)
    {
        var props = Directory.GetFiles(rootDir, "Directory.Build.props", SearchOption.AllDirectories).FirstOrDefault()
            ?? Directory.GetFiles(rootDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

        if (props == null)
            return "net8.0";

        var text = File.ReadAllText(props);
        var match = Regex.Match(text, @"<TargetFramework>([^<]+)</TargetFramework>");
        return match.Success ? match.Groups[1].Value : "net8.0";
    }

    private static int ExtractMajorVersion(string targetFramework)
    {
        var match = Regex.Match(targetFramework, @"net(\d+)\.");
        return match.Success ? int.Parse(match.Groups[1].Value) : 8;
    }
}
