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
using NUnitTern.Utils;

namespace NUnitTern.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeIgnoreAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateAttributeIgnoreCS";
        public const string Title = "Migrate attribute Ignore to NUnit 3";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate Attribute for Ignore property or IgnoreAttribute";
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

            if (attributeSyntax.Name.ToString() != "Ignore"
                && attributeSyntax.Name.ToString() != "TestCase"
                && attributeSyntax.Name.ToString() != "TestFixture"
                && attributeSyntax.Name.ToString() != "IgnoreAttribute"
                && attributeSyntax.Name.ToString() != "TestCaseAttribute"
                && attributeSyntax.Name.ToString() != "TestFixtureAttribute")
            {
                return;
            }

            var ignoreManager = new IgnoreRelatedAttributeManager(attributeSyntax);
            if (!ignoreManager.DoesIgnoringNeedAdjustment)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, attributeSyntax.GetLocation()));
        }
    }
}
