using NUnitTern.Utils.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace NUnitTern.Utils
{
    public class ExpectedExceptionTestCaseEquivalence
    {
        public ExceptionExpectancyAtAttributeLevel[] EquivalentItems { get; }

        private ExpectedExceptionTestCaseEquivalence(IEnumerable<ExceptionExpectancyAtAttributeLevel> equivalentItems)
        {
            EquivalentItems = equivalentItems.ToArray();
        }

        public static ExpectedExceptionTestCaseEquivalence[] CreateMany(ExpectedExceptionMethodMigrator migrator)
        {
            // degenerated case of a single ExpectedException for a test method
            if (migrator.ExceptionRelatedAttributes.Length == 1
                && migrator.ExceptionRelatedAttributes[0] is ExpectedExceptionAttribute)
            {
                return new[]
                {
                    new ExpectedExceptionTestCaseEquivalence(new[]
                    {
                        migrator.ExceptionRelatedAttributes[0]
                    })
                };
            }

            return migrator.ExceptionRelatedAttributes.OfType<TestCaseExpectingExceptionAttribute>()
                .GroupBy(tc => tc, Comparer.Instance)
                .Select(g => new ExpectedExceptionTestCaseEquivalence(g))
                .ToArray();
        }

        private class Comparer : IEqualityComparer<ExceptionExpectancyAtAttributeLevel>
        {
            public static readonly Comparer Instance = new Comparer();

            public bool Equals(ExceptionExpectancyAtAttributeLevel x, ExceptionExpectancyAtAttributeLevel y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.AssertedExceptionType.ToString() == y.AssertedExceptionType.ToString()
                       && x.ExpectedMessage == y.ExpectedMessage
                       && EffectiveMatchType(x.MatchType) == EffectiveMatchType(y.MatchType);
            }

            public int GetHashCode(ExceptionExpectancyAtAttributeLevel obj)
            {
                return new
                {
                    typeString = obj.AssertedExceptionType.ToString(),
                    obj.ExpectedMessage,
                    matchType = EffectiveMatchType(obj.MatchType)
                }.GetHashCode();
            }

            private static string EffectiveMatchType(string matchType)
            {
                return matchType ?? "Exact";
            }
        }
    }
}
