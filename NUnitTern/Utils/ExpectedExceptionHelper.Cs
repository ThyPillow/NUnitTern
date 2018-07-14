using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NUnitTern.Utils
{
    public class ExpectedExceptionHelper
    {
        public static bool TryFindDiagnosticLocation(MethodDeclarationSyntax methodSyntax, out Location diagnosticLocation)
        {
            var eligibleAttributes = GetEligibleAttributes(methodSyntax);
            if (eligibleAttributes.Length <= 0)
            {
                diagnosticLocation = null;
                return false;
            }

            diagnosticLocation = eligibleAttributes.Length == 1
                ? eligibleAttributes.First().GetLocation()
                : methodSyntax.Identifier.GetLocation();

            return true;
        }

        private static AttributeSyntax[] GetEligibleAttributes(BaseMethodDeclarationSyntax method)
        {
            var expectedExceptionAttributes = GetExpectedExceptionAttributes(method.AttributeLists);
            var testCaseExpectedExceptionAttributes = GetTestCaseAttributeWithExpectedException(method.AttributeLists, expectedExceptionAttributes.Any());

            return expectedExceptionAttributes.Union(testCaseExpectedExceptionAttributes).ToArray();
        }

        public static IEnumerable<AttributeSyntax> GetExpectedExceptionAttributes(SyntaxList<AttributeListSyntax> attrs)
        {
            return attrs
                .SelectMany(x => x.Attributes)
                .Where(x => x.Name.ToString() == "ExpectedException" || x.Name.ToString() == "ExpectedExceptionAttribute");
        }

        public static IEnumerable<AttributeSyntax> GetTestCaseAttributeWithExpectedException(SyntaxList<AttributeListSyntax> attrs,
            bool alreadyHasExpectedExceptionAttribute = false)
        {
            return attrs
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.Name.ToString() == "TestCase" || x.Name.ToString() == "TestCaseAttribute")
                    .Where(x => alreadyHasExpectedExceptionAttribute
                        || x.ArgumentList.Arguments.Any(a =>
                            a.NameEquals.Name.ToString() == "ExpectedExceptionName" || a.NameEquals.Name.ToString() == "ExpectedException"));
        }

        public static IEnumerable<AttributeSyntax> GetExpectedExceptionFreeTestCaseAttribute(SyntaxList<AttributeListSyntax> attrs,
            bool alreadyHasExpectedExceptionAttribute = false)
        {
            return attrs
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.Name.ToString() == "TestCase" || x.Name.ToString() == "TestCaseAttribute")
                    .Where(x => !alreadyHasExpectedExceptionAttribute
                        && x.ArgumentList.Arguments.All(a =>
                            a.NameEquals.Name.ToString() != "ExpectedExceptionName" && a.NameEquals.Name.ToString() != "ExpectedException"));
        }

    }
}
