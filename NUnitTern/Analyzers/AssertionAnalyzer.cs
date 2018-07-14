using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using NUnitTern.Utils;

namespace NUnitTern.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateAssertsCS";
        private const string Title = "Migrate assertion to NUnit 3";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate Assertion";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var containerNode = (InvocationExpressionSyntax)context.Node;
            var memberAccess = containerNode.Expression as MemberAccessExpressionSyntax;

            if (memberAccess == null || !MemberAccessMigrationTable.AssertHasFix(memberAccess))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation()));
        }

        private bool TryGetMemberAccess(InvocationExpressionSyntax container,
                out MemberAccessExpressionSyntax memberAccess)
        {
            memberAccess = container.Expression as MemberAccessExpressionSyntax;
            return memberAccess != null;
        }
    }
}
