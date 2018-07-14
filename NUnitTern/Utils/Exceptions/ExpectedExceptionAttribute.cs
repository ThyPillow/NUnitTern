using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils.Exceptions
{
    public class ExpectedExceptionAttribute : ExceptionExpectancyAtAttributeLevel
    {
        public ExpectedExceptionAttribute(AttributeSyntax attribute) : base(attribute)
        {
            SyntaxHelper.ParseAttributeArguments(attribute, ParseAttributeArgumentSyntax);
        }

        private void ParseAttributeArgumentSyntax(string nameEquals, ExpressionSyntax expression)
        {
            if (nameEquals != null
                && nameEquals != "UserMessage"
                && nameEquals != "Handler")
                return;

            switch (expression)
            {
                case LiteralExpressionSyntax literal when nameEquals == null && !IsLiteralNullOrEmpty(literal):
                    AssertedExceptionTypeName = literal.Token.ValueText;
                    break;
                case TypeOfExpressionSyntax typeOf:
                    AssertedExceptionTypeName = typeOf.Type.ToString();
                    break;
                case LiteralExpressionSyntax literal when nameEquals == "UserMessage":
                    UserMessage = literal.Token.ValueText;
                    break;
                case LiteralExpressionSyntax literal when nameEquals == "Handler":
                    HandlerName = literal.Token.ValueText;
                    break;
            }
        }

    }
}
