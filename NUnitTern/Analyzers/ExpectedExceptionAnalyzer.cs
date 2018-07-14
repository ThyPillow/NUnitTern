using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnitTern.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTern.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExpectedExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrateExpectedExceptionCS";
        public const string Title = "Migrate ExpectedException to replace it to NUnit 3 Assert.Throws<>";
        private const string MessageFormat = "Can be migrated";
        private const string Description = "Migrate ExpecedException (remove)";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodSyntax = (MethodDeclarationSyntax)context.Node;
            if (ExpectedExceptionHelper.TryFindDiagnosticLocation(methodSyntax, out var diagnosticLocation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, diagnosticLocation));
            }
        }
    }
}
