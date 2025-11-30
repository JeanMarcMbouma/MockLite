using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace BbQ.MockLite.Generators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockLiteAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            DiagnosticDescriptors.UnusedSetup,
            DiagnosticDescriptors.AsyncReturnsMismatch,
            DiagnosticDescriptors.NonVirtualClassMethod,
            DiagnosticDescriptors.VerifyNonMockedMethod,
            DiagnosticDescriptors.AmbiguousOverload,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register syntax node actions
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol == null) return;

        // Ignore all analysis on auto-generated code ([GeneratedMock] attribute)
        if (symbol.ContainingType.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == "GeneratedMockAttribute"))
        {
            return;
        }

        var name = symbol.Name;

        // ML002: AsyncReturnsMismatch
        if (name.StartsWith("Returns") && symbol.ReturnType is INamedTypeSymbol ret &&
            (ret.Name == "Task" || ret.Name == "ValueTask"))
        {
            if (!name.Contains("Async"))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.AsyncReturnsMismatch,
                    invocation.GetLocation()));
            }
        }

        // ML003: NonVirtualClassMethod
        if (symbol.ContainingType.TypeKind == TypeKind.Class &&
            !symbol.IsVirtual && !symbol.IsAbstract &&
            symbol.DeclaredAccessibility == Accessibility.Public)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NonVirtualClassMethod,
                invocation.GetLocation()));
        }

        // ML005: AmbiguousOverload
        var overloads = symbol.ContainingType.GetMembers(symbol.Name)
            .OfType<IMethodSymbol>().Count();
        if (overloads > 1 && !name.Contains('_')) // generator adds signature hash
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.AmbiguousOverload,
                invocation.GetLocation()));
        }

        // ML001 + ML004 require flow analysis (simplified here)
        // ML001: UnusedSetup — detect SetupXxx calls not followed by invocation
        if (name.StartsWith("Setup"))
        {
            // In a real analyzer, track symbol usage across the syntax tree
            // Here we flag as info for demonstration
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnusedSetup,
                invocation.GetLocation()));
        }

        // ML004: VerifyNonMockedMethod — detect VerifyXxx calls without prior Setup
        if (name.StartsWith("Verify"))
        {
            // In a real analyzer, track Setup calls; simplified here
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.VerifyNonMockedMethod,
                invocation.GetLocation()));
        }
    }
}
