using System.Linq;
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

        public static bool ReturnsReference(this IMethodSymbol self) => self.RefKind != RefKind.None;

        public static bool ReturnsExplicitReference(this IMethodSymbol self)
        {
            return self.IsExtensionMethod &&
                   self.ReturnsReference() &&
                   self.Is(RefListIndexer, DynReturnsSelf);
        }

        public static bool IsNoCopyReturn(this IMethodSymbol self) => self.Is(NoCopyReturn);
        public static bool IsNonAllocatedResultRef(this IMethodSymbol self) => self.Is(NonAllocatedResult);
        public static bool IsRefListIndexer(this IMethodSymbol self) => self.Is(RefListIndexer);
        public static bool IsDynReturnsSelf(this IMethodSymbol self) => self.Is(DynReturnsSelf);

        private static bool Is(this IMethodSymbol self, string attributeName)
        {
            return self.GetAttributes().Any(a => a.Is(attributeName));
        }

        private static bool Is(this IMethodSymbol self, string a1, string a2)
        {
            return self.GetAttributes().Any(a => a.Is(a1) || a.Is(a2));
        }

        public static bool IsNoCopyType(this ITypeSymbol self) => self.Is(NoCopy);
        public static bool IsDealloc(this ITypeSymbol self) => self.Is(Dealloc);
        public static bool IsRefList(this ITypeSymbol self) => self.Is(RefList);
        public static bool IsUnmanagedRefList(this ITypeSymbol self) => self.IsUnmanagedType && self.IsRefList();
        public static bool IsDeallocOrRefList(this ITypeSymbol self) => self.Is(Dealloc, RefList);
        public static bool IsDynSized(this ITypeSymbol self) => self.Is(DynSized);

        private static bool Is(this ITypeSymbol self, string attributeName)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
        }

        private static bool Is(this ITypeSymbol self, string a1, string a2)
        {
            if (self.TypeKind != TypeKind.Struct)
                return false;

            return self.GetAttributes().Any(a => a.Is(a1) || a.Is(a2));
        }

        private static bool Is(this AttributeData attribute, string attributeName)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == attributeName + Suffix;
        }

        public static bool IsDynNoResize(this IParameterSymbol self) => self.Is(DynNoResize);

        private static bool Is(this IParameterSymbol self, string attributeName)
        {
            return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
        }
    }
}