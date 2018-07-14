using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnitTern.Analyzers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, ExpectedExceptionAnalyzer.Title)]
    public class ExpectedExceptionFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate ExpectedException to NUnit 3";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ExpectedExceptionAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var methodToBeFixed = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var fix = new ExpectedExceptionCodeAction(document, methodToBeFixed);

            context.RegisterCodeFix(fix, context.Diagnostics);
        }
    }
}
