using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTern.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeReplaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateAttributeReplacementCS";
        public const string Title = "Migrate attribute to replace them to NUnit 3 specific ones";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate Attribute (replace)";
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

            if (attributeSyntax.Name.ToString() != "RequiresMTA"
                && attributeSyntax.Name.ToString() != "RequiresSTA"
                && attributeSyntax.Name.ToString() != "TestFixtureSetUp"
                && attributeSyntax.Name.ToString() != "TestFixtureTearDown"
                && attributeSyntax.Name.ToString() != "RequiresMTAAttribute"
                && attributeSyntax.Name.ToString() != "RequiresSTAAttribute"
                && attributeSyntax.Name.ToString() != "TestFixtureSetUpAttribute"
                && attributeSyntax.Name.ToString() != "TestFixtureSetUpAttribute")
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, attributeSyntax.GetLocation()));
        }
    }
}