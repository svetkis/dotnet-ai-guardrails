// TRAP: Кастомный Roslyn-анализатор ломается после обновления Roslyn или начинает давать false positives.
// GUARDRAIL: Unit-тесты на анализатор с positive/negative cases.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Этот паттерн — шаблон. Замени YourAnalyzer на реальный анализатор проекта
//       и адаптируй positive/negative cases под его диагностики.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TUnit;

namespace Tests.Patterns;

public class AnalyzerTests
{
    // TRAP: Анализатор не срабатывает на нарушении.
    // GUARDRAIL: Positive case — код с нарушением должен породить diagnostic.
    [Test]
    public async Task YourAnalyzer_FlagsViolation()
    {
        const string code = """
            namespace DemoProject.Domain;

            public class Booking
            {
                public long Id { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync<YourAnalyzer>(code);

        await Assert.That(diagnostics)
            .Contains(d => d.Id == "SAE001")
            .Because("The analyzer must report SAE001 for the positive case.");
    }

    // TRAP: Анализатор срабатывает там, где не должен (false positive).
    // GUARDRAIL: Negative case — корректный код не порождает diagnostic.
    [Test]
    public async Task YourAnalyzer_IgnoresValidCode()
    {
        const string code = """
            namespace DemoProject.Domain;

            public readonly record struct BookingId(long Value);

            public class Booking
            {
                public BookingId Id { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync<YourAnalyzer>(code);

        await Assert.That(diagnostics)
            .IsEmpty()
            .Because("The analyzer must not produce diagnostics for valid code.");
    }

    private static async Task<IReadOnlyList<Diagnostic>> RunAnalyzerAsync<T>(string sourceCode)
        where T : DiagnosticAnalyzer, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(GetSystemRuntimeReference())
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (compilationErrors.Count > 0)
            throw new InvalidOperationException($"Analyzer test input failed to compile: {string.Join(Environment.NewLine, compilationErrors)}");

        var analyzer = new T();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static string GetSystemRuntimeReference()
    {
        // Reference assemblies live in the .NET SDK packs directory.
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var version = Path.GetFileName(runtimeDir);
        var dotnetRoot = Directory.GetParent(Directory.GetParent(runtimeDir)!.Parent!.FullName)!.FullName;

        var refAssemblyPath = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref", version, "ref", "net10.0", "System.Runtime.dll");
        if (File.Exists(refAssemblyPath))
            return refAssemblyPath;

        throw new InvalidOperationException($"Could not locate System.Runtime.dll reference assembly. Expected: {refAssemblyPath}");
    }
}

// TODO: Replace this placeholder with your real analyzer.
public class YourAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SAE001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Primitive identifier in domain entity",
        messageFormat: "Property '{0}' is a primitive identifier",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)context.Node;
        if (property.Identifier.Text == "Id" && property.Type.ToString() == "long")
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, property.Identifier.GetLocation(), property.Identifier.Text));
        }
    }
}
