using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace NUnitTern
{
    public class BreakingChange
    {
        public DiagnosticAnalyzer Analyzer { get; set; }
        public CodeFixProvider CodeFix { get; set; }
        public List<String> DiagnosticIds { get; set; }
        public String EquivalenceKey { get; set; }
    }
}
