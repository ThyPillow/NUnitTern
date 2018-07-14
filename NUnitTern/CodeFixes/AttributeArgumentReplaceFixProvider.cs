using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnitTern.Analyzers;

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, AttributeArgumentReplaceAnalyzer.Title)]
    public class AttributeArgumentReplaceFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate Attribute Argument to NUnit 3";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AttributeArgumentReplaceAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostics = context.Diagnostics.Where(d => d.Id == AttributeArgumentReplaceAnalyzer.DiagnosticId).ToArray();
            if (!diagnostics.Any())
                return;

            var attributeArgumentSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeArgumentSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MigrateAttributeArgument(context.Document, root, attributeArgumentSyntax, c),
                    equivalenceKey: Title),
                diagnostics.First());
        }

        private Task<Document> MigrateAttributeArgument(Document document, SyntaxNode root, AttributeArgumentSyntax attributeArgumentSyntax, CancellationToken c)
        {
            var target = "ExpectedResult";
            var newArgument = SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals(target),
                attributeArgumentSyntax.NameColon, attributeArgumentSyntax.Expression);

            var newRoot = root.ReplaceNode(attributeArgumentSyntax, newArgument);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
