using Microsoft.CodeAnalysis;
using static Ksi.Roslyn.SymbolNames;

namespace Ksi.Roslyn
{
    public static class SymbolExtensions
    {
        public static bool TryGetGenericArg(this INamedTypeSymbol self, out INamedTypeSymbol? genericType)
        {
            genericType = null;

            if (!self.IsGenericType)
                return false;

            if (self.TypeParameters.Length != 1)
                return false;

            if (self.TypeArguments[0] is not INamedTypeSymbol gta)
                return false;

            genericType = gta;
            return true;
        }

        public static bool IsNoCopyType(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (attribute.IsNoCopy())
                    return true;
            }

            return false;
        }

        public static bool IsNoCopyReturnMethod(this IMethodSymbol method)
        {
            foreach (var attribute in method.GetAttributes())
            {
                if (attribute.IsNoCopyReturn())
                    return true;
            }

            return false;
        }

        public static bool IsNonAllocatedResultRef(this IMethodSymbol method)
        {
            if (!method.ReturnsByRef)
                return false;

            foreach (var attribute in method.GetAttributes())
            {
                if (attribute.IsNonAllocatedResult())
                    return true;
            }

            return false;
        }

        public static bool IsUnmanagedRefListType(this ITypeSymbol self)
        {
            return self.IsUnmanagedType && self.IsRefListType();
        }

        public static bool IsRefListType(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (attribute.IsRefList())
                    return true;
            }

            return false;
        }

        public static bool IsDeallocType(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (attribute.IsDealloc())
                    return true;
            }

            return false;
        }

        public static bool IsDeallocOrRefListType(this ITypeSymbol self)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            foreach (var attribute in self.GetAttributes())
            {
                if (attribute.IsDealloc() || attribute.IsRefList())
                    return true;
            }

            return false;
        }

        private static bool IsNoCopy(this AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == NoCopy + Suffix;
        }

        private static bool IsNoCopyReturn(this AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == NoCopyReturn + Suffix;
        }

        private static bool IsDealloc(this AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == Dealloc + Suffix;
        }

        private static bool IsRefList(this AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == RefList + Suffix;
        }

        private static bool IsNonAllocatedResult(this AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == NonAllocatedResult + Suffix;
        }
    }
}