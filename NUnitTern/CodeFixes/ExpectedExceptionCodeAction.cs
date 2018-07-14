using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnitTern.Utils;
using NUnitTern.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NUnitTern.CodeFixes
{
    public class ExpectedExceptionCodeAction : CodeAction
    {
        public override string Title => ExpectedExceptionFixProvider.Title;
        public override string EquivalenceKey => ExpectedExceptionFixProvider.Title;

        private Document document;
        private MethodDeclarationSyntax methodToBeFixed;
        private ExpectedExceptionMethodMigrator migrator;
        private ExpectedExceptionTestCaseEquivalence[] equivalences;
        private SyntaxTriviaList methodLineSeparator;

        public ExpectedExceptionCodeAction(Document document, MethodDeclarationSyntax methodToBeFixed)
        {
            this.document = document;
            this.methodToBeFixed = methodToBeFixed;
            migrator = new ExpectedExceptionMethodMigrator(methodToBeFixed);
            equivalences = ExpectedExceptionTestCaseEquivalence.CreateMany(migrator);
            methodLineSeparator = GetMethodLineSeparator(methodToBeFixed);
        }

        private SyntaxTriviaList GetMethodLineSeparator(MethodDeclarationSyntax method)
        {
            var endOfLineTrivia = method.DescendantTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia));
            var endOfLine = endOfLineTrivia != default(SyntaxTrivia)
                ? endOfLineTrivia
                : SyntaxFactory.CarriageReturnLineFeed;
            var lineBreak = new[] { endOfLine, endOfLine };
            return SyntaxFactory.TriviaList(lineBreak);
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var fixedTestMethods = ProduceFixedTestMethods();
            root = root.ReplaceNode(methodToBeFixed, fixedTestMethods);
            return document.WithSyntaxRoot(root);
        }

        private IEnumerable<MethodDeclarationSyntax> ProduceFixedTestMethods()
        {
            var fixedMethods = new List<MethodDeclarationSyntax>();
            var testMethodNamer = new TestMethodNamer(methodToBeFixed, migrator.ExceptionFreeTestCaseAttributeNodes.Any());

            if (TryProduceExceptionUnrelatedTestMethod(out MethodDeclarationSyntax fixedExceptionUnrelatedMethod))
            {
                fixedMethods.Add(fixedExceptionUnrelatedMethod);
            }
            foreach (var equivalence in equivalences)
            {
                fixedMethods.Add(ProduceTestMethodForTestCaseWithExpectedExceptionEquivalence(equivalence, equivalences.Length, testMethodNamer));
            }

            return fixedMethods;
        }

        private bool TryProduceExceptionUnrelatedTestMethod(out MethodDeclarationSyntax fixedExceptionUnrelatedMethod)
        {
            if (!migrator.ExceptionFreeTestCaseAttributeNodes.Any())
            {
                fixedExceptionUnrelatedMethod = null;
                return false;
            }
            fixedExceptionUnrelatedMethod = methodToBeFixed.WithoutExceptionExpectancyInAttributes(migrator.ExceptionFreeTestCaseAttributeNodes).WithTrailingTrivia(methodLineSeparator);
            return true;
        }

        private MethodDeclarationSyntax ProduceTestMethodForTestCaseWithExpectedExceptionEquivalence(
            ExpectedExceptionTestCaseEquivalence equivalence, int equivalenceCount, TestMethodNamer testMethodNamer)
        {
            var testCasesToRemain = equivalence.EquivalentItems.Select(i => i.AttributeNode).ToArray();
            var exceptionExpectancy = equivalence.EquivalentItems.First();

            var clusterMethod = methodToBeFixed.WithoutExceptionExpectancyInAttributes(testCasesToRemain)
                .WithBody(CreateAssertedBlock(exceptionExpectancy)).WithTrailingTrivia(CreateClusterMethodTrailingTrivia(equivalence))
                .WithIdentifier(testMethodNamer.CreateName(exceptionExpectancy, equivalenceCount));

            return clusterMethod;
        }

        private BlockSyntax CreateAssertedBlock(ExceptionExpectancyAtAttributeLevel exceptionExpectancy)
        {
            return exceptionExpectancy.GetAssertExceptionBlockCreator()
                .Create(methodToBeFixed, exceptionExpectancy.AssertedExceptionType).WithAdditionalAnnotations(Formatter.Annotation);
        }

        private SyntaxTriviaList CreateClusterMethodTrailingTrivia(ExpectedExceptionTestCaseEquivalence equivalence)
        {
            return equivalence == equivalences.Last()
                ? methodToBeFixed.GetTrailingTrivia()
                : methodLineSeparator;
        }

        private class TestMethodNamer
        {
            private readonly MethodDeclarationSyntax _originalTestMethod;
            private readonly bool _doExceptionUnrelatedTCsExist;
            private readonly Dictionary<string, int> _methodNamesCount;

            public TestMethodNamer(MethodDeclarationSyntax originalTestMethod, bool doExceptionUnrelatedTCsExist)
            {
                _originalTestMethod = originalTestMethod;
                _doExceptionUnrelatedTCsExist = doExceptionUnrelatedTCsExist;
                _methodNamesCount = new Dictionary<string, int>();
            }

            public SyntaxToken CreateName(ExceptionExpectancyAtAttributeLevel attribute, int clustersCount)
            {
                return clustersCount > 1 ||
                       clustersCount == 1 && _doExceptionUnrelatedTCsExist
                    ? CreateClusterTestMethodName(attribute)
                    : _originalTestMethod.Identifier;
            }

            private SyntaxToken CreateClusterTestMethodName(ExceptionExpectancyAtAttributeLevel attribute)
            {
                var exceptionTypeName = attribute.AssertedExceptionType.ToString().Split('.').Last();
                var proposedName = $"{_originalTestMethod.Identifier}_ShouldThrow{exceptionTypeName}";
                _methodNamesCount.TryGetValue(proposedName, out int actualMethodCount);
                _methodNamesCount[proposedName] = ++actualMethodCount;

                return actualMethodCount == 1
                    ? SyntaxFactory.ParseToken(proposedName)
                    : SyntaxFactory.ParseToken(proposedName + actualMethodCount);
            }
        }

    }
}
