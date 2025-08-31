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

        public static bool TryGetGenericArg(this INamedTypeSymbol self, out INamedTypeSymbol? genericType)
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

        public static bool IsUnmanagedRefList(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct || !self.IsUnmanagedType)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (AttributeUtil.IsRefList(attribute))
                    return true;
            }

            return false;
        }

        public static bool HasDeallocAttribute(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (AttributeUtil.IsDealloc(attribute))
                    return true;
            }

            return false;
        }

        public static bool HasDeallocOrRefListAttribute(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (AttributeUtil.IsDealloc(attribute) || AttributeUtil.IsRefList(attribute))
                    return true;
            }

            return false;
        }
    }
}