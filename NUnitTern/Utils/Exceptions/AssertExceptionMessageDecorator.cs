using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils.Exceptions
{
    public class AssertExceptionMessageDecorator : AssertExceptionBlockDecorator
    {
        private readonly ExceptionExpectancyAtAttributeLevel _attribute;

        public AssertExceptionMessageDecorator(IAssertExceptionBlockCreator blockCreator,
            ExceptionExpectancyAtAttributeLevel attribute) : base(blockCreator)
        {
            _attribute = attribute;
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            return body.AddStatements(CreateAssertExceptionMessageStatement());
        }

        private IEnumerable<ArgumentSyntax> CreateExceptionMessageAssertThatArguments()
        {
            return new[]
            {
                SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(AssignAssertThrowsToLocalVariableDecorator
                            .LocalExceptionVariableName),
                        SyntaxFactory.IdentifierName(nameof(Exception.Message)))),

                SyntaxFactory.Argument(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(CreateDoesOrIs()),
                            SyntaxFactory.IdentifierName(
                                CreateExpectedExceptionMessageAssertionMethod())),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.ParseToken($"\"{_attribute.ExpectedMessage}\""
                                        )))))))
            };
        }

        private string CreateDoesOrIs() => 
            _attribute.MatchType == null 
            || _attribute.MatchType == "Exact"
                ? "Is" 
                : "Does";

        private string CreateExpectedExceptionMessageAssertionMethod()
        {
            var matchType = _attribute.MatchType;

            switch (matchType)
            {
                case "Contains": return "Contain";
                case "Regex": return "Match";
                case "StartsWith": return "StartWith";
                default: return "EqualTo";
            }
        }

        private ExpressionStatementSyntax CreateAssertExceptionMessageStatement()
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Assert"),
                        SyntaxFactory.IdentifierName("That")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            CreateExceptionMessageAssertThatArguments()))));
        }
    }
}