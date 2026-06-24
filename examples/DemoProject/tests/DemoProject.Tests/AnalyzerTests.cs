// GUARDRAIL: Кастомные Roslyn-анализаторы имеют positive/negative unit-тесты.
// Этот файл — рабочая адаптация шаблона из tests/patterns/AnalyzerTests.cs

using System.Collections.Immutable;
using DemoProject.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TUnit;

namespace DemoProject.Tests;

public class AnalyzerTests
{
    [Test]
    public async Task StrongTypedIdAnalyzer_FlagsPrimitiveIdInDomainEntity()
    {
        const string code = """
            namespace DemoProject.Domain;

            public class Booking
            {
                public long Id { get; set; }
            }
            """;

        await AssertSingleDiagnosticAsync(code, StronglyTypedIdAnalyzer.PropertyDiagnosticId, "long");
    }

    [Test]
    public async Task StrongTypedIdAnalyzer_FlagsRawGuidParameterInDomainMethod()
    {
        const string code = """
            using System;

            namespace DemoProject.Domain;

            public class BookingService
            {
                public void Load(Guid bookingId)
                {
                }
            }
            """;

        await AssertSingleDiagnosticAsync(code, StronglyTypedIdAnalyzer.ParameterDiagnosticId, "Guid");
    }

    [Test]
    public async Task StrongTypedIdAnalyzer_IgnoresTypedId()
    {
        const string code = """
            namespace DemoProject.Domain;

            public readonly record struct BookingId(long Value);

            public class Booking
            {
                public BookingId Id { get; set; }
            }
            """;

        await AssertNoAnalyzerDiagnosticsAsync(code);
    }

    [Test]
    public async Task StrongTypedIdAnalyzer_IgnoresTypedIdParameter()
    {
        const string code = """
            using System;

            namespace DemoProject.Domain;

            public readonly record struct BookingId(Guid Value);

            public class BookingService
            {
                public void Load(BookingId bookingId)
                {
                }
            }
            """;

        await AssertNoAnalyzerDiagnosticsAsync(code);
    }

    private static async Task AssertSingleDiagnosticAsync(string sourceCode, string expectedId, string expectedSnippet)
    {
        var diagnostics = await RunAnalyzerAsync(sourceCode);

        await Assert.That(diagnostics.Count)
            .IsEqualTo(1)
            .Because("The positive analyzer case should produce exactly one diagnostic.");

        var diagnostic = diagnostics.Single();
        await Assert.That(diagnostic.Id)
            .IsEqualTo(expectedId)
            .Because("The analyzer should report the expected diagnostic ID.");

        var expectedLocation = FindSnippetLocation(sourceCode, expectedSnippet);
        var actualLocation = diagnostic.Location.GetLineSpan().StartLinePosition;

        await Assert.That(actualLocation.Line)
            .IsEqualTo(expectedLocation.Line)
            .Because("The diagnostic should point to the exact line of the offending syntax.");

        await Assert.That(actualLocation.Character)
            .IsEqualTo(expectedLocation.Character)
            .Because("The diagnostic should point to the exact column of the offending syntax.");
    }

    private static async Task AssertNoAnalyzerDiagnosticsAsync(string sourceCode)
    {
        var diagnostics = await RunAnalyzerAsync(sourceCode);

        await Assert.That(diagnostics)
            .IsEmpty()
            .Because("Valid strongly typed ID usage must not trigger analyzer diagnostics.");
    }

    private static async Task<IReadOnlyList<Diagnostic>> RunAnalyzerAsync(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (compilationErrors.Count > 0)
            throw new InvalidOperationException($"Analyzer test input failed to compile: {string.Join(Environment.NewLine, compilationErrors)}");

        var analyzer = new StronglyTypedIdAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static (int Line, int Character) FindSnippetLocation(string sourceCode, string snippet)
    {
        var snippetIndex = sourceCode.IndexOf(snippet, StringComparison.Ordinal);
        if (snippetIndex < 0)
            throw new InvalidOperationException($"Could not find snippet '{snippet}' in analyzer test source.");

        var line = 0;
        var character = 0;

        for (var i = 0; i < snippetIndex; i++)
        {
            if (sourceCode[i] == '\n')
            {
                line++;
                character = 0;
            }
            else if (sourceCode[i] != '\r')
            {
                character++;
            }
        }

        return (line, character);
    }
}
