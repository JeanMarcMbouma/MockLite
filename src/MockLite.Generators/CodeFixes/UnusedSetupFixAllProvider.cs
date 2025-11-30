using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BbQ.MockLite.Generators.CodeFixes;

internal sealed class UnusedSetupFixAllProvider : FixAllProvider
{
    public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        var solution = fixAllContext.Solution;
        var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document!).ConfigureAwait(false);

        var actions = diagnostics.Select(d =>
            CodeAction.Create("Remove all unused Setups",
                ct => RemoveAllSetups(fixAllContext.Document!, diagnostics, ct),
                nameof(UnusedSetupFixAllProvider)));

        return actions.FirstOrDefault();
    }

    private async Task<Document> RemoveAllSetups(Document document, IEnumerable<Diagnostic> diagnostics, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct);
        var nodes = diagnostics.Select(d => root!.FindNode(d.Location.SourceSpan)).OfType<InvocationExpressionSyntax>();
        var newRoot = root!.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia);
        return document.WithSyntaxRoot(newRoot!);
    }
}