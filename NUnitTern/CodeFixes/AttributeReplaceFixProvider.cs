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

namespace NUnitTern.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, AttributeReplaceAnalyzer.Title)]
    public class AttributeReplaceFixProvider : CodeFixProvider
    {
        public static string Title = "Migrate attributes that needs to be replaced to NUnit 3 ones";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AttributeReplaceAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();

            context.RegisterCodeFix(new ReplaceDeprecatedAttribute(document, root, attributeSyntax),
                context.Diagnostics);

        }
    }

    public class ReplaceDeprecatedAttribute : CodeAction
    {
        private readonly Document _document;
        private readonly SyntaxNode _root;
        private readonly AttributeSyntax _attributeSyntax;
        private readonly string _targetString;
        private readonly string _targetFamily;

        public sealed override string EquivalenceKey => AttributeReplaceFixProvider.Title;

        public sealed override string Title => AttributeReplaceFixProvider.Title;

        public ReplaceDeprecatedAttribute(Document document, SyntaxNode root, AttributeSyntax attributeSyntax)
        {
            _document = document;
            _root = root;
            _attributeSyntax = attributeSyntax;
            _targetString =
                GetRefinedAttributeWithItsMigrationTarget(_attributeSyntax)
                    .replaceWith;
            _targetFamily = _targetString.Substring(0, 5);
        }


        internal static (string lookupKey, string replaceWith) GetRefinedAttributeWithItsMigrationTarget(
            AttributeSyntax attributeSyntax)
        {
            var attrName = attributeSyntax.Name.ToString();
            var nsFreeName = attrName.Substring(attrName.LastIndexOf('.') + 1);
            var refinedName = nsFreeName.Replace("Attribute", string.Empty);
            var replaceWith = ReplacementTable[refinedName];

            return (refinedName, replaceWith);
        }

        private static readonly IReadOnlyDictionary<string, string> ReplacementTable = new Dictionary<string, string>
        {
            ["RequiresMTA"] = "Apartment(System.Threading.ApartmentState.MTA)",
            ["RequiresSTA"] = "Apartment(System.Threading.ApartmentState.STA)",
            ["TestFixtureSetUp"] = "OneTimeSetUp",
            ["TestFixtureTearDown"] = "OneTimeTearDown",
        };

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var nameAndArgumentsList = _targetString.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            var newAttributeName = SyntaxFactory.ParseName(nameAndArgumentsList[0]);

            AttributeSyntax newAttribute;
            var thereIsNameOnlyAndNoArgumentsList = nameAndArgumentsList.Length == 1;
            if (thereIsNameOnlyAndNoArgumentsList)
            {
                newAttribute = SyntaxFactory.Attribute(newAttributeName);
            }
            else
            {
                var argumentsList = $"({nameAndArgumentsList[1]}"; // the ending ')' remains after the initial split
                newAttribute = SyntaxFactory.Attribute(newAttributeName,
                    SyntaxFactory.ParseAttributeArgumentList(argumentsList));
            }

            newAttribute = newAttribute.WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = _root.ReplaceNode(_attributeSyntax, newAttribute);

            return Task.FromResult(_document.WithSyntaxRoot(newRoot));
        }
    }
}
