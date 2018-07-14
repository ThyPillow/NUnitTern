using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnitTern.Analyzers;
using NUnitTern.Utils;

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class ConstraintFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate constraint to NUnit 3";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConstraintAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostics = context.Diagnostics.Where(d => d.Id == ConstraintAnalyzer.DiagnosticId).ToArray();
            if (!diagnostics.Any())
                return;

            var memberAccessExpression = root.FindNode(context.Span).FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MigrateConstraint(context.Document, memberAccessExpression, c),
                    equivalenceKey: Title),
                diagnostics.First());
        }

        private async Task<Document> MigrateConstraint(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);
            var fixedInvocation = CreateFixedContainer(memberAccessExpression);
            var fixedMemberAccessContainer = fixedInvocation.WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(memberAccessExpression, fixedMemberAccessContainer);

            return document.WithSyntaxRoot(newRoot);
        }

        public MemberAccessExpressionSyntax CreateFixedContainer(MemberAccessExpressionSyntax container)
        {
            if (!MemberAccessMigrationTable.TryGetConstraintFixExpression(container, out ExpressionSyntax fixExpression))
                return container;

            if (container.Name is GenericNameSyntax genericName)
                fixExpression = SyntaxFactory.ParseExpression($"{fixExpression}{genericName.TypeArgumentList}");

            return fixExpression as MemberAccessExpressionSyntax ?? container;
        }
    }
}
