using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnitTern.Utils.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace NUnitTern.Utils
{
    public class ExpectedExceptionMethodMigrator
    {
        public AttributeSyntax[] ExceptionFreeTestCaseAttributeNodes { get; }
        public ExceptionExpectancyAtAttributeLevel[] ExceptionRelatedAttributes { get; }

        public ExpectedExceptionMethodMigrator(MethodDeclarationSyntax method)
        {
            var allAttributes = method.AttributeLists.SelectMany(x => x.Attributes).ToList();
            var attributes = new List<ExceptionExpectancyAtAttributeLevel>();

            var isExpectedException = TryGetFirstExpectedExceptionAttribute(method.AttributeLists,
                out ExpectedExceptionAttribute expectedException);
            if (isExpectedException)
            {
                attributes.Add(expectedException);
            }

            var exceptionRelatedTestCases = GetExceptionRelatedTestCases(method.AttributeLists, isExpectedException, expectedException);
            attributes.AddRange(exceptionRelatedTestCases);
            ExceptionRelatedAttributes = attributes.ToArray();
            ExceptionFreeTestCaseAttributeNodes = ExpectedExceptionHelper.GetExpectedExceptionFreeTestCaseAttribute(method.AttributeLists, isExpectedException).ToArray();
        }

        private TestCaseExpectingExceptionAttribute[] GetExceptionRelatedTestCases(SyntaxList<AttributeListSyntax> attributes, bool isExpectedException, ExpectedExceptionAttribute expectedException)
        {
            return ExpectedExceptionHelper.GetTestCaseAttributeWithExpectedException(attributes, isExpectedException)
                .Select(x => new TestCaseExpectingExceptionAttribute(x, expectedException))
                .ToArray();
        }

        public static bool TryGetFirstExpectedExceptionAttribute(SyntaxList<AttributeListSyntax> attributes, out ExpectedExceptionAttribute expectedException)
        {
            var expectedExceptionNode = ExpectedExceptionHelper.GetExpectedExceptionAttributes(attributes).FirstOrDefault();
            if (expectedExceptionNode != null)
            {
                expectedException = new ExpectedExceptionAttribute(expectedExceptionNode);
                return true;
            }
            expectedException = null;
            return false;
        }
    }
}
