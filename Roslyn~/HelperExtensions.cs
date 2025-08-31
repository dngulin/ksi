using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DnDev.Roslyn
{
    public static class HelperExtensions
    {
        public static bool IsUnmanagedConstraint(this TypeParameterConstraintSyntax self)
        {
            return self.IsKind(SyntaxKind.TypeConstraint) && self is TypeConstraintSyntax tcs && tcs.Type.IsUnmanaged;
        }

        public static bool TryGetGenericArg(this INamedTypeSymbol self, out INamedTypeSymbol genericType)
        {
            genericType = null;

            if (!self.IsGenericType)
                return false;

            if (self.TypeParameters.Length != 1)
                return false;

            if (!(self.TypeArguments[0] is INamedTypeSymbol gta))
                return false;

            genericType = gta;
            return true;
        }
    }
}