using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NUnitTern
{
    public class Tern
    {
        private MSBuildWorkspace _workspace;
        private string _projectFilePath;

        public Tern(MSBuildWorkspace workspace, string projectFilePath)
        {
            _workspace = workspace;
            _projectFilePath = projectFilePath;
        }

        public void Migrate(BreakingChange breakingChange)
        {
            var stopwatch = Stopwatch.StartNew();
            var project = GetOrLoadProject();

            var documentDiagnosticsMap = ComputeDiagnostics(project, breakingChange);
            Console.WriteLine($"{breakingChange.EquivalenceKey} - Analysis done in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"{breakingChange.EquivalenceKey} - Found {documentDiagnosticsMap.Sum(x => x.Value.Count())} counts of diagnostics to fix");

            if (documentDiagnosticsMap.Any())
            {
                ApplyFixes(documentDiagnosticsMap, breakingChange);
            }
            Console.WriteLine($"{breakingChange.EquivalenceKey} - Finished migration in {stopwatch.ElapsedMilliseconds}ms (analysis and fix)");
            Console.WriteLine();
        }

        private Project GetOrLoadProject()
        {
            var project = _workspace.CurrentSolution?.Projects.FirstOrDefault(x => x.FilePath == _projectFilePath);
            if (project == null)
            {
                project = _workspace.OpenProjectAsync(_projectFilePath).Result;
            }
            return project;
        }

        private Dictionary<Document, List<Diagnostic>> ComputeDiagnostics(Project project, BreakingChange breakingChange)
        {
            var documentDiagnosticsMap = new Dictionary<Document, List<Diagnostic>>();
            var compilation = project.GetCompilationAsync().Result;
            var diagnosticResults = compilation.WithAnalyzers(ImmutableArray.Create(breakingChange.Analyzer)).GetAnalyzerDiagnosticsAsync().Result;
            var interestingResults = diagnosticResults.Where(x => x.Severity != DiagnosticSeverity.Hidden).ToArray();
            
            foreach (var diagnostic in interestingResults)
            {
                if (diagnostic.Severity != DiagnosticSeverity.Hidden)
                {
                    var doc = project.GetDocument(diagnostic.Location.SourceTree);
                    if (!documentDiagnosticsMap.ContainsKey(doc))
                    {
                        documentDiagnosticsMap[doc] = new List<Diagnostic>();
                    }
                    documentDiagnosticsMap[doc].Add(diagnostic);
                }
            }
            return documentDiagnosticsMap;
        }

        private void ApplyFixes(Dictionary<Document, List<Diagnostic>> documentDiagnosticsMap, BreakingChange breakingChange)
        {
            if (!documentDiagnosticsMap.Any())
            {
                return;
            }
            var cancellationToken = new CancellationToken();
            var diagnosticProvider = new CustomDiagnosticProvider(documentDiagnosticsMap);

            var fixAllContext = new FixAllContext(documentDiagnosticsMap.First().Key, breakingChange.CodeFix, FixAllScope.Project,
                breakingChange.EquivalenceKey, breakingChange.DiagnosticIds, diagnosticProvider, cancellationToken);

            var fixes = WellKnownFixAllProviders.BatchFixer.GetFixAsync(fixAllContext).Result;
            var operations = fixes.GetOperationsAsync(cancellationToken).Result;
            foreach (var operation in operations)
            {
                operation.Apply(_workspace, cancellationToken);
            }
        }
    }
}
