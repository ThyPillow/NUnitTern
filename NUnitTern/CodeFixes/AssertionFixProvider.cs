using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AssertionFixProvider))]
    public class AssertionFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate assertion to NUnit 3";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AssertionAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostics = context.Diagnostics.Where(d => d.Id == AssertionAnalyzer.DiagnosticId).ToArray();
            if (!diagnostics.Any())
                return;

            var invocationExpression = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MigrateAssertion(context.Document, invocationExpression, c),
                    equivalenceKey: Title),
                diagnostics.First());
        }

        private async Task<Document> MigrateAssertion(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);
            var fixedInvocation = CreateFixedContainer(invocationExpression);
            var fixedMemberAccessContainer = fixedInvocation.WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(invocationExpression, fixedMemberAccessContainer);

            return document.WithSyntaxRoot(newRoot);
        }

        public InvocationExpressionSyntax CreateFixedContainer(InvocationExpressionSyntax container)
        {
            if (!TryGetMemberAccess(container, out var memberAccess))
                return container;

            if (!MemberAccessMigrationTable.TryGetAssertFixExpression(memberAccess, out ExpressionSyntax fixExpression))
                return container;

            var fixedInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression("Assert.That"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        container.ArgumentList.Arguments.Insert(1, SyntaxFactory.Argument(fixExpression)))));

            return fixedInvocation;
        }

        private bool TryGetMemberAccess(InvocationExpressionSyntax container,
                out MemberAccessExpressionSyntax memberAccess)
        {
            memberAccess = container.Expression as MemberAccessExpressionSyntax;
            return memberAccess != null;
        }
    }
}
