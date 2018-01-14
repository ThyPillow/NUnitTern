using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace AutoRefactoringWithRoslyn
{
    public class CustomDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        private Dictionary<Document, List<Diagnostic>> documentDiagnosticsMap;

        public CustomDiagnosticProvider(Dictionary<Document, List<Diagnostic>> documentDiagnosticsMap)
        {
            this.documentDiagnosticsMap = documentDiagnosticsMap;
        }

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            var allDiagnostics = documentDiagnosticsMap.Values.SelectMany(x => x);
            return Task.FromResult((IEnumerable<Diagnostic>)allDiagnostics);
        }

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            IEnumerable<Diagnostic> documentDiagnostics = documentDiagnosticsMap[document];
            return Task.FromResult(documentDiagnostics);
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<Diagnostic>());
        }
    }
}