using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils.Exceptions
{
    public abstract class AssertExceptionBlockDecorator : IAssertExceptionBlockCreator
    {
        private readonly IAssertExceptionBlockCreator _blockCreator;

        protected AssertExceptionBlockDecorator(IAssertExceptionBlockCreator blockCreator)
        {
            _blockCreator = blockCreator;
        }

        public virtual BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            return _blockCreator.Create(method, assertedType);
        }
    }
}