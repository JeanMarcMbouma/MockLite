using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MockLite.Generators.CodeFixes;

internal sealed class VerifyNonMockedFixAllProvider : FixAllProvider
{
    public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document!).ConfigureAwait(false);

        var actions = diagnostics.Select(d =>
            CodeAction.Create("Remove all invalid Verifies",
                ct => RemoveAllVerifies(fixAllContext.Document!, diagnostics, ct),
                nameof(VerifyNonMockedFixAllProvider)));
        return actions.FirstOrDefault();
    }

    private async Task<Document> RemoveAllVerifies(Document document, IEnumerable<Diagnostic> diagnostics, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct);
        var nodes = diagnostics.Select(d => root!.FindNode(d.Location.SourceSpan)).OfType<InvocationExpressionSyntax>();
        var newRoot = root!.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia);
        return document.WithSyntaxRoot(newRoot!);
    }
}