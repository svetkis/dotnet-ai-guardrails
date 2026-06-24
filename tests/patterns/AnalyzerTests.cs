// TRAP: Кастомный Roslyn-анализатор ломается после обновления Roslyn или начинает давать false positives.
// GUARDRAIL: Unit-тесты на анализатор с positive/negative cases.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Требуется пакет Microsoft.CodeAnalysis.CSharp.Testing (или аналог).
//       Этот паттерн — шаблон; замените YourAnalyzer на реальный анализатор проекта.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using TUnit;

namespace Tests.Patterns;

public class AnalyzerTests
{
    // TRAP: Анализатор не срабатывает на нарушении.
    // GUARDRAIL: Positive case — код с нарушением должен породить diagnostic.
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

        var test = new CSharpAnalyzerTest<YourAnalyzer, DefaultVerifier>
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        test.ExpectedDiagnostics.Add(
            DiagnosticResult
                .CompilerError("SAE001")
                .WithSpan(5, 16, 5, 18)
                .WithArguments("Id"));

        await test.RunAsync();
    }

    // TRAP: Анализатор срабатывает там, где не должен (false positive).
    // GUARDRAIL: Negative case — корректный код не порождает diagnostic.
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

        var test = new CSharpAnalyzerTest<YourAnalyzer, DefaultVerifier>
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        await test.RunAsync();
    }
}

// Placeholder: replace with your real analyzer.
public class YourAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;
    public override void Initialize(AnalysisContext context) { }
}
