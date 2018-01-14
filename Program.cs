using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRefactoringWithRoslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            // Running the program will add the const modifier to solutionFilePath
            string solutionFilePath = @"D:\Documents\GitHub\RefactoringWithRoslyn\AutoRefactoringWithRoslyn.sln";

            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionFilePath).Result;

            var documentDiagnosticsMap = new Dictionary<Document, List<Diagnostic>>();
            var cancellationToken = new CancellationToken();

            var analyzerCodeFixMap = ImmutableArray.CreateBuilder<(ImmutableArray<DiagnosticAnalyzer> Analyzers, CodeFixProvider CodeFix)>();
            analyzerCodeFixMap.Add((ImmutableArray.Create(new ConstAnalyzer() as DiagnosticAnalyzer), new ConstCodeFixProvider()));

            foreach (var project in solution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;
                foreach (var analyzerCodeFix in analyzerCodeFixMap)
                {
                    var diagnosticResults = compilation.WithAnalyzers(analyzerCodeFix.Analyzers).GetAnalyzerDiagnosticsAsync().Result;
                    var interestingResults = diagnosticResults.Where(x => x.Severity != DiagnosticSeverity.Hidden).ToArray();
                    if (interestingResults.Any())
                    {
                        Console.WriteLine($"\n\nResults for analyzers {analyzerCodeFix.Analyzers}");
                        Underline();
                    }

                    foreach (var diagnostic in interestingResults)
                    {
                        if (diagnostic.Severity != DiagnosticSeverity.Hidden)
                        {
                            var doc = project.GetDocument(diagnostic.Location.SourceTree);
                            Console.WriteLine(doc.FilePath);
                            if (!documentDiagnosticsMap.ContainsKey(doc))
                            {
                                documentDiagnosticsMap[doc] = new List<Diagnostic>();
                            }
                            documentDiagnosticsMap[doc].Add(diagnostic);
                            Console.WriteLine($"Severity: {diagnostic.Severity}\tMessage: {diagnostic.GetMessage()}");
                        }
                    }

                    var diagnosticProvider = new CustomDiagnosticProvider(documentDiagnosticsMap);
                    foreach (var kvp in documentDiagnosticsMap)
                    {
                        var fixAllContext = new FixAllContext(kvp.Key, analyzerCodeFix.CodeFix, FixAllScope.Document, ConstCodeFixProvider.Title,
                            new List<string>() { ConstAnalyzer.DiagnosticId }, diagnosticProvider, cancellationToken);

                        var fixes = WellKnownFixAllProviders.BatchFixer.GetFixAsync(fixAllContext).Result;
                        var operations = fixes.GetOperationsAsync(cancellationToken).Result;
                        foreach (var operation in operations)
                        {
                            operation.Apply(workspace, cancellationToken);
                        }
                    }
                }
            }

            Underline();
            Console.WriteLine("End of diagnostics and fixes");
            Console.ReadLine();
        }

        private static void Underline()
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine();
        }
    }
}
