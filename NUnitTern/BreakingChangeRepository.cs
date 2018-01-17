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
                            Analyzer = new ConstAnalyzer(),
                            CodeFix = new ConstCodeFixProvider(),
                            DiagnosticIds = new List<String>() { ConstAnalyzer.DiagnosticId },
                            EquivalenceKey = ConstCodeFixProvider.Title
                        },
                    };
                }
                return _breakingChanges;
            }
        }

    }
}
