using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BbQ.MockLite.Generators.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AmbiguousOverloadCodeFixProvider))]
[Shared]
public class AmbiguousOverloadCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ["ML005"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var node = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
        if (node == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use overload‑specific Setup/Verify",
                ct => SuggestOverloadSpecificMethod(context.Document, node, ct),
                nameof(AmbiguousOverloadCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> SuggestOverloadSpecificMethod(Document document, InvocationExpressionSyntax node, CancellationToken ct)
    {
        var memberAccess = node.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return document;

        // Append a signature hint (developer must adjust manually)
        var newName = SyntaxFactory.IdentifierName(memberAccess.Name.Identifier.Text + "_Overload");
        var newMemberAccess = memberAccess.WithName(newName);
        var newNode = node.WithExpression(newMemberAccess);

        var root = await document.GetSyntaxRootAsync(ct);
        var newRoot = root!.ReplaceNode(node, newNode);
        return document.WithSyntaxRoot(newRoot);
    }
}