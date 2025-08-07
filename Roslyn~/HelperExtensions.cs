using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefListRoslyn
{
    public static class HelperExtensions
    {
        public static bool Contains(this in SyntaxList<AttributeListSyntax> self, string attributeName)
        {
            foreach (var attrList in self)
            foreach (var attr in attrList.Attributes)
            {
                if (attr.Name.ToString() == attributeName)
                    return true;
            }

            return false;
        }

        public static bool IsUnmanagedConstraint(this TypeParameterConstraintSyntax self)
        {
            return self.IsKind(SyntaxKind.TypeConstraint) && self is TypeConstraintSyntax tcs && tcs.Type.IsUnmanaged;
        }

        public static bool Contains(this in ImmutableArray<AttributeData> self, string attributeName)
        {
            foreach (var attr in self)
            {
                if (attr.AttributeClass != null && attr.AttributeClass.Name == attributeName)
                    return true;
            }

            return false;
        }

        public static bool IsGenericOver(this INamedTypeSymbol self, string attributeName)
        {
            if (!self.IsGenericType)
                return false;

            if (self.TypeParameters.Length != 1)
                return false;

            return self.TypeArguments[0].GetAttributes().Contains(attributeName);
        }
    }
}