using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DemoProject.Analyzers;

// TRAP: Тест собирается и зеленеет, но не проверяет обещанное поведение:
//       zero-assert, IsNotNull()-only, assertion внутри if, тавтология.
// GUARDRAIL: Анализатор SAE006-SAE009 ловит не-валидирующие тесты на этапе
//            компиляции, до запуска. Снижает fault sensitivity риск в CI.
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NonValidatingTestAnalyzer : DiagnosticAnalyzer
{
    public const string MustAssertDiagnosticId = "SAE006";
    public const string NullOnlyDiagnosticId = "SAE007";
    public const string BypassedDiagnosticId = "SAE008";
    public const string TautologicalDiagnosticId = "SAE009";

    private const string Category = "Design";

    private static readonly DiagnosticDescriptor MustAssertRule = new(
        id: MustAssertDiagnosticId,
        title: "Test method must contain an assertion",
        messageFormat: "Test method '{0}' does not contain any assertion or verification",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A test that never asserts proves nothing. Add an assertion that checks an observable postcondition.");

    private static readonly DiagnosticDescriptor NullOnlyRule = new(
        id: NullOnlyDiagnosticId,
        title: "Test asserts only non-null",
        messageFormat: "Test method '{0}' checks only null / not-null; it cannot verify the concrete behavior promised by its name",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Non-null is too weak when the test name promises a concrete value, state, or effect.");

    private static readonly DiagnosticDescriptor BypassedRule = new(
        id: BypassedDiagnosticId,
        title: "Assertion can be bypassed on the successful path",
        messageFormat: "Test method '{0}' has assertions that are not guaranteed to execute on every successful path",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An assertion inside a conditional branch that may be skipped does not verify the behavior on the green path.");

    private static readonly DiagnosticDescriptor TautologicalRule = new(
        id: TautologicalDiagnosticId,
        title: "Assertion is tautological",
        messageFormat: "Assertion cannot fail by construction",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Tautological assertions (e.g., x == x, literal true) give false safety.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MustAssertRule, NullOnlyRule, BypassedRule, TautologicalRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static IEnumerable<InvocationExpressionSyntax> FilterAssertionChains(IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        var innerAssertionSpans = new HashSet<TextSpan>(assertions
            .Where(a => a.Expression is MemberAccessExpressionSyntax)
            .Select(a => ((MemberAccessExpressionSyntax)a.Expression).Expression.Span));

        return assertions.Where(a => !innerAssertionSpans.Contains(a.Span));
    }

    private static IReadOnlyList<string> GetAdditionalAssertionPrefixes(SyntaxNodeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        if (!options.TryGetValue("dotnet_diagnostic.SAE006.additional_assertion_prefixes", out var raw) ||
            string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToArray();
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (symbol is null || !TestAssertionHelper.IsTestMethod(symbol))
            return;

        var additionalPrefixes = GetAdditionalAssertionPrefixes(context);

        // Collect assertions in the method body only, excluding nested lambdas /
        // local functions whose execution is not guaranteed by the test path.
        var assertionInvocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => !TestAssertionHelper.IsInsideNestedFunction(inv))
            .Where(inv => TestAssertionHelper.IsAssertionInvocation(inv, context.SemanticModel, additionalPrefixes))
            .ToList();

        assertionInvocations = FilterAssertionChains(assertionInvocations).ToList();

        if (assertionInvocations.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MustAssertRule, method.Identifier.GetLocation(), method.Identifier.Text));
            return;
        }

        // SAE007: null-only behavior check.
        if (assertionInvocations.All(inv => TestAssertionHelper.IsNullOnlyAssertion(inv)))
        {
            context.ReportDiagnostic(Diagnostic.Create(NullOnlyRule, method.Identifier.GetLocation(), method.Identifier.Text));
        }

        // SAE008: no assertion guaranteed on every successful path.
        if (!HasGuaranteedAssertion(method, context.SemanticModel, assertionInvocations))
        {
            var location = assertionInvocations[0].GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(BypassedRule, location, method.Identifier.Text));
        }

        // SAE009: tautological assertions.
        foreach (var assertion in assertionInvocations.Where(a => TestAssertionHelper.IsTautologicalAssertion(a, context.SemanticModel)))
        {
            context.ReportDiagnostic(Diagnostic.Create(TautologicalRule, assertion.GetLocation()));
        }
    }

    // Uses the compiler control-flow graph to prove that every successful path
    // from entry to exit passes through at least one assertion. If the CFG is
    // unavailable (e.g., expression-bodied members in older Roslyn), falls back
    // to a conservative syntax approximation.
    private static bool HasGuaranteedAssertion(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        // CFG for async methods includes state-machine branches that make a
        // simple entry→exit proof unreliable; use the syntax fallback there.
        if (method.Modifiers.Any(SyntaxKind.AsyncKeyword))
            return FallbackHasGuaranteedAssertion(method, assertions);

        var cfg = ControlFlowGraph.Create(method, semanticModel);
        if (cfg is null)
            return FallbackHasGuaranteedAssertion(method, assertions);

        var assertionBlocks = new HashSet<BasicBlock>();

        foreach (var block in cfg.Blocks)
        {
            if (block.Operations.Any(op => op is not null && assertions.Any(a => op.Syntax.Span.Contains(a.Span))))
                assertionBlocks.Add(block);
        }

        var entryBlock = cfg.Blocks.FirstOrDefault(b => b.Kind == BasicBlockKind.Entry);
        var exitBlock = cfg.Blocks.FirstOrDefault(b => b.Kind == BasicBlockKind.Exit);
        if (entryBlock is null || exitBlock is null)
            return FallbackHasGuaranteedAssertion(method, assertions);

        // Backward reachability from the exit block, refusing to traverse
        // assertion blocks. If the entry block is reachable without crossing an
        // assertion, a green path bypasses every check.
        var reachableWithoutAssert = new HashSet<BasicBlock>();
        var stack = new Stack<BasicBlock>();
        stack.Push(exitBlock);

        while (stack.Count > 0)
        {
            var block = stack.Pop();
            if (!reachableWithoutAssert.Add(block))
                continue;

            if (assertionBlocks.Contains(block))
                continue;

            foreach (var predecessor in block.Predecessors)
            {
                if (predecessor.Source is not null)
                    stack.Push(predecessor.Source);
            }
        }

        return !reachableWithoutAssert.Contains(entryBlock);
    }

    private static bool FallbackHasGuaranteedAssertion(MethodDeclarationSyntax method, IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        if (method.ExpressionBody is not null)
            return ExpressionContainsAssertion(method.ExpressionBody.Expression, assertions);

        if (method.Body is null)
            return false;

        return method.Body.Statements.Any(s => StatementGuaranteesAssertion(s, assertions));
    }

    private static bool StatementGuaranteesAssertion(StatementSyntax statement, IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        if (IsDirectAssertionStatement(statement, assertions))
            return true;

        switch (statement)
        {
            case BlockSyntax block:
                return block.Statements.Any(s => StatementGuaranteesAssertion(s, assertions));

            case IfStatementSyntax ifStatement when ifStatement.Else is not null:
                return StatementGuaranteesAssertion(ifStatement.Statement, assertions) &&
                       StatementGuaranteesAssertion(ifStatement.Else.Statement, assertions);

            case TryStatementSyntax tryStatement:
                return StatementGuaranteesAssertion(tryStatement.Block, assertions) ||
                       (tryStatement.Finally is not null && StatementGuaranteesAssertion(tryStatement.Finally.Block, assertions));

            case SwitchStatementSyntax switchStatement:
                return switchStatement.Sections.Any(s => s.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel))) &&
                       switchStatement.Sections.All(s => SwitchSectionGuaranteesAssertion(s, assertions));

            default:
                return false;
        }
    }

    private static bool SwitchSectionGuaranteesAssertion(SwitchSectionSyntax section, IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        return section.Statements.Any(s => StatementGuaranteesAssertion(s, assertions));
    }

    private static bool IsDirectAssertionStatement(StatementSyntax statement, IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        return statement is ExpressionStatementSyntax expressionStatement &&
               ExpressionContainsAssertion(expressionStatement.Expression, assertions);
    }

    private static bool ExpressionContainsAssertion(ExpressionSyntax expression, IReadOnlyList<InvocationExpressionSyntax> assertions)
    {
        return assertions.Any(a => expression.Span.Contains(a.Span));
    }
}
