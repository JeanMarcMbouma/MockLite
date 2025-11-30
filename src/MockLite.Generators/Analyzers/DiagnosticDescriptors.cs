// src/MockLite.Generators/Generators/Diagnostics.cs
using Microsoft.CodeAnalysis;

namespace MockLite.Generators.Analyzers;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnusedSetup =
        new("ML001", "Unused Setup",
            "Setup defined but method never invoked in test code",
            "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AsyncReturnsMismatch =
        new("ML002", "Async Returns Mismatch",
            "Async method should use ReturnsAsync instead of Returns",
            "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NonVirtualClassMethod =
        new("ML003", "Non-mockable class method",
            "Attempted to mock a non-virtual class method. Consider mocking the interface or using adapter.",
            "Usage", DiagnosticSeverity.Info, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor VerifyNonMockedMethod =
        new("ML004", "Verify Non-Mocked Method",
            "Verify called on a method that was never mocked",
            "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AmbiguousOverload =
        new("ML005", "Ambiguous Overload",
            "Multiple overloads exist; use generated overload-specific Setup/Verify methods",
            "Usage", DiagnosticSeverity.Info, isEnabledByDefault: true);
}