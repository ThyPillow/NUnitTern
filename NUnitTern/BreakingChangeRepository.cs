using NUnitTern.Analyzers;
using NUnitTern.CodeFixes;
using System;
using System.Collections.Generic;

namespace NUnitTern
{
    public static class BreakingChangeRepository
    {
        private static List<BreakingChange> _breakingChanges = null;
        public static List<BreakingChange> BreakingChanges
        {
            get
            {
                if (_breakingChanges == null)
                {
                    _breakingChanges = new List<BreakingChange>
                    {
                        new BreakingChange()
                        {
                            Analyzer = new AttributeArgumentReplaceAnalyzer(),
                            CodeFix = new AttributeArgumentReplaceFixProvider(),
                            DiagnosticIds = new List<String>() { AttributeArgumentReplaceAnalyzer.DiagnosticId },
                            EquivalenceKey = AttributeArgumentReplaceFixProvider.Title
                        },
                        new BreakingChange()
                        {
                            Analyzer = new AttributeReplaceAnalyzer(),
                            CodeFix = new AttributeReplaceFixProvider(),
                            DiagnosticIds = new List<String>() { AttributeReplaceAnalyzer.DiagnosticId },
                            EquivalenceKey = AttributeReplaceFixProvider.Title
                        },
                        new BreakingChange()
                        {
                            Analyzer = new AssertionAnalyzer(),
                            CodeFix = new AssertionFixProvider(),
                            DiagnosticIds = new List<String>() { AssertionAnalyzer.DiagnosticId },
                            EquivalenceKey = AssertionFixProvider.Title
                        },
                        new BreakingChange()
                        {
                            Analyzer = new ConstraintAnalyzer(),
                            CodeFix = new ConstraintFixProvider(),
                            DiagnosticIds = new List<String>() { ConstraintAnalyzer.DiagnosticId },
                            EquivalenceKey = ConstraintFixProvider.Title
                        },
                        new BreakingChange()
                        {
                            Analyzer = new AttributeIgnoreAnalyzer(),
                            CodeFix = new AttributeIgnoreFixProvider(),
                            DiagnosticIds = new List<String>() { AttributeIgnoreAnalyzer.DiagnosticId },
                            EquivalenceKey = AttributeIgnoreFixProvider.Title
                        },
                        new BreakingChange()
                        {
                            Analyzer = new ExpectedExceptionAnalyzer(),
                            CodeFix = new ExpectedExceptionFixProvider(),
                            DiagnosticIds = new List<string>() { ExpectedExceptionAnalyzer.DiagnosticId },
                            EquivalenceKey = ExpectedExceptionFixProvider.Title
                        }
                    };
                }
                return _breakingChanges;
            }
        }

    }
}
