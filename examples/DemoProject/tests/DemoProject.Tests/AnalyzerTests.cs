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

        var diagnostics = await RunAnalyzerAsync(code);

        await Assert.That(diagnostics)
            .Contains(d => d.Id == StronglyTypedIdAnalyzer.PropertyDiagnosticId)
            .Because("Primitive 'Id' property in Domain must trigger SAE001.");
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

        var diagnostics = await RunAnalyzerAsync(code);

        await Assert.That(diagnostics)
            .IsEmpty()
            .Because("Strongly typed 'Id' property must not trigger any diagnostic.");
    }

    private static async Task<IReadOnlyList<Diagnostic>> RunAnalyzerAsync(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new StronglyTypedIdAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
