using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnitTern.Analyzers;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstCodeFixProvider))]
    public class ConstCodeFixProvider : CodeFixProvider
    {
        public static string Title = "Make constant";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ConstAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> MakeConstAsync(Document document, LocalDeclarationStatementSyntax declaration, CancellationToken c)
        {    // Remove the leading trivia from the local declaration.
            var firstToken = declaration.GetFirstToken();
            var leadingTrivia = firstToken.LeadingTrivia;
            var trimmedLocal = declaration.ReplaceToken(
                firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

            // Create a const token with the leading trivia.
            var constToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            // Insert the const token into the modifiers list, creating a new modifiers list.
            var newModifiers = trimmedLocal.Modifiers.Insert(0, constToken);

            // Produce the new local declaration.
            var newLocal = trimmedLocal.WithModifiers(newModifiers);

            // Add an annotation to format the new local declaration.
            var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(c);
            var newRoot = oldRoot.ReplaceNode(declaration, formattedLocal);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
