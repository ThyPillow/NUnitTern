using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTern.Utils
{
    internal static class MemberAccessMigrationTable
    {
        private static readonly IImmutableDictionary<string, ExpressionSyntax> ConstraintFixMap =
            new Dictionary<string, ExpressionSyntax>
            {
                ["Text.All"] = Parse("Is.All"),
                ["Text.Contains"] = Parse("Does.Contain"),
                ["Text.DoesNotContain"] = Parse("Does.Not.Contain"),
                ["Text.StartsWith"] = Parse("Does.StartWith"),
                ["Text.DoesNotStartWith"] = Parse("Does.Not.StartWith"),
                ["Text.EndsWith"] = Parse("Does.EndWith"),
                ["Text.DoesNotEndWith"] = Parse("Does.Not.EndWith"),
                ["Text.Matches"] = Parse("Does.Match"),
                ["Text.DoesNotMatch"] = Parse("Does.Not.Match"),
                ["Is.StringStarting"] = Parse("Does.StartWith"),
                ["Is.StringEnding"] = Parse("Does.EndWith"),
                ["Is.StringContaining"] = Parse("Does.Contain"),
                ["Is.StringMatching"] = Parse("Does.Match"),
                ["Is.InstanceOfType"] = Parse("Is.InstanceOf"),
            }.ToImmutableDictionary();

        private static readonly IImmutableDictionary<string, ExpressionSyntax> AssertFixMap =
            new Dictionary<string, ExpressionSyntax>
            {
                ["Assert.IsNull"] = Parse("Is.Null"),
                ["Assert.IsNotNull"] = Parse("Is.Not.Null"),
                ["Assert.IsNullOrEmpty"] = Parse("Is.Null.Or.Empty"),
                ["Assert.IsNotNullOrEmpty"] = Parse("Is.Not.Null.And.Not.Empty")
            }.ToImmutableDictionary();

        internal static bool TryGetAssertFixExpression(MemberAccessExpressionSyntax memberAccess,
            out ExpressionSyntax fixExpression)
        {
            var memberAccessMethodName = memberAccess.Name.Identifier.Text;
            fixExpression = null;
            if (!AssertMethodNames.Contains(memberAccessMethodName))
            {
                return false;
            }

            var lookupName = $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";
            if (!AssertFixMap.TryGetValue(lookupName, out fixExpression))
            {
                return false;
            }

            return true;
        }

        internal static bool AssertHasFix(MemberAccessExpressionSyntax memberAccess)
        {
            var memberAccessMethodName = memberAccess.Name.Identifier.Text;
            if (!AssertMethodNames.Contains(memberAccessMethodName))
            {
                return false;
            }

            var lookupName = $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";
            return AssertFixMap.ContainsKey(lookupName);
        }

        internal static bool TryGetConstraintFixExpression(MemberAccessExpressionSyntax memberAccess,
            out ExpressionSyntax fixExpression)
        {
            var memberAccessMethodName = memberAccess.Name.Identifier.Text;
            fixExpression = null;
            if (!ConstraintMethodNames.Contains(memberAccessMethodName))
            {
                return false;
            }

            var lookupName = $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";
            if (!ConstraintFixMap.TryGetValue(lookupName, out fixExpression))
            {
                return false;
            }

            return true;
        }

        internal static bool ConstraintHasFix(MemberAccessExpressionSyntax memberAccess)
        {
            var memberAccessMethodName = memberAccess.Name.Identifier.Text;
            if (!ConstraintMethodNames.Contains(memberAccessMethodName))
            {
                return false;
            }

            var lookupName = $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";
            return ConstraintFixMap.ContainsKey(lookupName);
        }

        private static readonly IImmutableSet<string> AssertMethodNames =
            AssertFixMap
            .Keys
            .Select(k => k.Split('.')[1])
            .ToImmutableHashSet();

        private static readonly IImmutableSet<string> ConstraintMethodNames =
            ConstraintFixMap
            .Keys
            .Select(k => k.Split('.')[1])
            .ToImmutableHashSet();

        private static ExpressionSyntax Parse(string strExpression) => SyntaxFactory.ParseExpression(strExpression);
    }
}
