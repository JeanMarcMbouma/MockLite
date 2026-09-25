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
            DiagnosticDescriptors.AsyncReturnsMismatch,
            DiagnosticDescriptors.NonVirtualClassMethod,
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

        var containingType = symbol.ContainingType;

        // Skip analysis on Mock<T> runtime API calls — the runtime proxy handles
        // everything correctly and these diagnostics are meant for generated mocks.
        if (IsMockRuntimeType(containingType))
            return;

        // Skip analysis on auto-generated mock types ([GeneratedMock] attribute)
        if (containingType.GetAttributes().Any(attr =>
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

        // Diagnostics about the member selected inside a setup/verify expression must
        // only run when that member invocation is actually nested in a MockLite runtime
        // expression. Otherwise ordinary application calls can receive MockLite warnings.
        var insideMockLiteExpression = IsInsideMockLiteExpression(context, invocation);

        // ML003: NonVirtualClassMethod — only flag when the call target is a
        // non-virtual method on a concrete class that also implements interfaces.
        if (insideMockLiteExpression &&
            containingType.TypeKind == TypeKind.Class &&
            !symbol.IsVirtual && !symbol.IsAbstract &&
            symbol.DeclaredAccessibility == Accessibility.Public &&
            containingType.AllInterfaces.Length > 0)
        {
            // Only report when the method is declared on a class that implements
            // interfaces — a strong signal the user should mock the interface.
            var declaringInterface = containingType.AllInterfaces
                .FirstOrDefault(i => i.GetMembers(symbol.Name).OfType<IMethodSymbol>().Any());
            if (declaringInterface != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NonVirtualClassMethod,
                    invocation.GetLocation()));
            }
        }

        // ML005: AmbiguousOverload — only flag on interface method calls where
        // the interface defines multiple overloads with the same name.
        if (insideMockLiteExpression && containingType.TypeKind == TypeKind.Interface)
        {
            var overloads = containingType.GetMembers(symbol.Name)
                .OfType<IMethodSymbol>().Count();
            if (overloads > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.AmbiguousOverload,
                    invocation.GetLocation()));
            }
        }

        // ML001 + ML004 require full data-flow analysis to track Setup→Invoke and
        // Setup→Verify pairs across the method body.  Without proper flow analysis
        // these would produce false positives on every call, so they are left as
        // no-ops until a proper implementation is added.
    }


    private static bool IsInsideMockLiteExpression(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation)
    {
        var lambda = invocation.Ancestors().OfType<LambdaExpressionSyntax>().FirstOrDefault();
        if (lambda is null) return false;

        var outerInvocation = lambda.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (outerInvocation is null) return false;

        var outerSymbol = context.SemanticModel.GetSymbolInfo(outerInvocation).Symbol as IMethodSymbol;
        if (outerSymbol is null || !IsMockRuntimeType(outerSymbol.ContainingType)) return false;

        return outerSymbol.Name is "Setup" or "Verify" or "Throws" or "OnCall" or "SetupSequence";
    }

    /// <summary>
    /// Returns true when the type is the MockLite runtime Mock&lt;T&gt; class or its
    /// nested types (e.g. SetupPhrase).
    /// </summary>
    private static bool IsMockRuntimeType(INamedTypeSymbol? type)
    {
        while (type != null)
        {
            if (type.Name == "Mock" &&
                type.ContainingNamespace?.ToDisplayString() == "BbQ.MockLite")
                return true;
            type = type.ContainingType;
        }
        return false;
    }
}
