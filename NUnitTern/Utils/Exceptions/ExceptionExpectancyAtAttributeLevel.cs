using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTern.Utils.Exceptions
{
    public class ExceptionExpectancyAtAttributeLevel
    {
        private const string ImplicitAssertedExceptionTypeAssumedByDefault = "System.Exception";
        private const string ExpectExceptionHandlerMethodName = "HandleException";

        protected internal string AssertedExceptionTypeName;

        protected ExceptionExpectancyAtAttributeLevel(AttributeSyntax attribute)
        {
            AttributeNode = attribute ?? throw new ArgumentNullException(nameof(attribute));

            SyntaxHelper.ParseAttributeArguments(attribute, ParseAttributeArgumentSyntax);
            ParseTestFixtureClass(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>());
        }

        public AttributeSyntax AttributeNode { get; }

        public string ExpectedMessage { get; protected set; }

        public string MatchType { get; protected set; }

        public TypeSyntax AssertedExceptionType => SyntaxFactory.ParseTypeName(
            AssertedExceptionTypeName ?? ImplicitAssertedExceptionTypeAssumedByDefault);

        public string HandlerName { get; protected set; }

        public string UserMessage { get; protected set; }

        public IAssertExceptionBlockCreator GetAssertExceptionBlockCreator()
        {
            IAssertExceptionBlockCreator creator = new AssertThrowsExceptionCreator();

            if (ExpectedMessage != null || HandlerName != null)
                creator = new AssignAssertThrowsToLocalVariableDecorator(creator);

            if (ExpectedMessage != null)
                creator = new AssertExceptionMessageDecorator(creator, this);

            if (HandlerName != null)
                creator = new AssertHandlerMethodDecorator(creator, HandlerName);

            if (UserMessage != null)
                creator = new AssertUserMessageDecorator(creator, this);

            return creator;
        }

        private void ParseTestFixtureClass(BaseTypeDeclarationSyntax classDeclaration)
        {
            if (SyntaxHelper.GetAllBaseTypes(classDeclaration).Any(t => t.ToString() == "IExpectException"))
            {
                HandlerName = ExpectExceptionHandlerMethodName;
            }
        }

        protected static bool IsLiteralNullOrEmpty(LiteralExpressionSyntax literal)
        {
            return literal.Kind() == SyntaxKind.NullLiteralExpression || string.IsNullOrEmpty(literal.Token.ValueText);
        }

        private void ParseAttributeArgumentSyntax(string nameEquals, ExpressionSyntax expression)
        {
            if (nameEquals == null)
                return;

            switch (expression)
            {
                case LiteralExpressionSyntax literal when nameEquals == "ExpectedExceptionName"
                                                          && !IsLiteralNullOrEmpty(literal):
                    AssertedExceptionTypeName = literal.Token.ValueText;
                    break;
                case TypeOfExpressionSyntax typeOf when nameEquals == "ExpectedException":
                    AssertedExceptionTypeName = typeOf.Type.ToString();
                    break;
                case LiteralExpressionSyntax literal when nameEquals == "ExpectedMessage":
                    ExpectedMessage = literal.Token.ValueText;
                    break;
                case MemberAccessExpressionSyntax memberAccess when nameEquals == "MatchType":
                    MatchType = memberAccess.Name.ToString();
                    break;
            }
        }
    }
}
