using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils.Exceptions
{
    /// <summary>
    ///     Provides method body with assertion that it throws an exception of given type and/or asserts on its properties.
    /// </summary>
    public interface IAssertExceptionBlockCreator
    {
        BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType);
    }
}