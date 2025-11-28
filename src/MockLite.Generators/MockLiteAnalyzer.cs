// src/MockLite.Generators/Generators/Diagnostics.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace MockLite.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockLiteAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor NonVirtualClassMethod =
        new("ML003", "Non-mockable class method",
            "Attempted to mock a non-virtual class method. Consider mocking the interface or using adapter.",
            "Usage", DiagnosticSeverity.Info, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NonVirtualClassMethod);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        // For brevity: Hook syntax actions to detect GenerateMock on classes with non-virtual methods
    }
}