using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MockLite.Generators.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VerifyNonMockedMethodCodeFixProvider))]
[Shared]
public class VerifyNonMockedMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ["ML004"];

    public override FixAllProvider GetFixAllProvider() => new VerifyNonMockedFixAllProvider();

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var node = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
        if (node == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove invalid Verify",
                ct => RemoveVerify(context.Document, node, ct),
                nameof(VerifyNonMockedMethodCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> RemoveVerify(Document document, InvocationExpressionSyntax node, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct);
        var newRoot = root!.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
        return document.WithSyntaxRoot(newRoot!);
    }
}
