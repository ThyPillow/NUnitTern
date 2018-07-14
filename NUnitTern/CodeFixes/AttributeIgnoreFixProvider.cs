using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnitTern.Analyzers;
using NUnitTern.Utils;

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, AttributeIgnoreAnalyzer.Title)]
    public class AttributeIgnoreFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate Ignore related attributes and attribute property to NUnit 3";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AssertionAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostics = context.Diagnostics.Where(d => d.Id == AttributeIgnoreAnalyzer.DiagnosticId).ToArray();
            if (!diagnostics.Any())
                return;

            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();
            var mngr = new IgnoreRelatedAttributeManager(attributeSyntax);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MigrateAttribute(context.Document, root, mngr, c),
                    equivalenceKey: Title),
                diagnostics.First());
        }

        private Task<Document> MigrateAttribute(Document document, SyntaxNode root, IgnoreRelatedAttributeManager mngr, CancellationToken c)
        {
            var newRoot = root.ReplaceNode(mngr.Attribute, mngr.GetFixedAttribute());
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
