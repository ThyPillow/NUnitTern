using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils.Exceptions
{
    public class TestCaseExpectingExceptionAttribute : ExceptionExpectancyAtAttributeLevel
    {
        /// <summary>
        /// Constructs <c>NUnit.Framework.TestCaseAttribute</c> model that allows for sourcing exception related 
        /// properties from <c>NUnit.Framework.ExpectedExceptionAttribute</c> in case of not defining them by itself.
        /// </summary>
        public TestCaseExpectingExceptionAttribute(AttributeSyntax attribute,
            ExpectedExceptionAttribute expectedException) : base(attribute)
        {
            if (expectedException == null)
                return;

            if (AssertedExceptionTypeName == null)
                AssertedExceptionTypeName = expectedException.AssertedExceptionTypeName;

            if (ExpectedMessage == null)
                ExpectedMessage = expectedException.ExpectedMessage;

            if (MatchType == null)
                MatchType = expectedException.MatchType;

            if (HandlerName == null)
                HandlerName = expectedException.HandlerName;

            if (UserMessage == null)
                UserMessage = expectedException.UserMessage;
        }

    }
}
