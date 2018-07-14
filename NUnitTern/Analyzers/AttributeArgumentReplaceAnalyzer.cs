using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace NUnitTern.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeArgumentReplaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateAttributeArgumentReplacementCS";
        public const string Title = "Migrate attribute argument to replace them to NUnit 3 specific ones";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate Attribute argument (replace)";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeSyntax = (AttributeSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            if (attributeSyntax.Name.ToString() != "TestCase"
                && attributeSyntax.Name.ToString() != "TestCaseAttribute")
            {
                return;
            }

            if (attributeSyntax.ArgumentList == null || !attributeSyntax.ArgumentList.Arguments.Any())
            {
                return;
            }

            foreach (var argument in attributeSyntax.ArgumentList.Arguments)
            {
                var argumentName = argument.NameEquals?.Name?.Identifier.ValueText;
                if (argumentName == "Result")
                {
                    var argLocation = argument.Expression.Parent.GetLocation();

                    context.ReportDiagnostic(Diagnostic.Create(Rule, argLocation));
                    return;
                }
            }
        }
    }
}
