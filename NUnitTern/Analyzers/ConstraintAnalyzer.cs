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
    public class ConstraintAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateConstraintsCS";
        private const string Title = "Migrate constraint to NUnit 3";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate Constraint";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var containerNode = (MemberAccessExpressionSyntax)context.Node;

            if (!MemberAccessMigrationTable.ConstraintHasFix(containerNode))
            {
                return;
            }
            context.ReportDiagnostic(Diagnostic.Create(Rule, containerNode.GetLocation()));
        }
    }
}
