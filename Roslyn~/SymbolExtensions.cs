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

        public static bool ReturnsRef(this IMethodSymbol self) => self.RefKind is RefKind.Ref or RefKind.RefReadOnly;

        public static bool ReturnsRefOrWrappedRef(this IMethodSymbol self)
        {
            return self.ReturnsRef() || self.ReturnType.IsWrappedRef();
        }

        public static bool ReturnsRefPath(this IMethodSymbol self)
        {
            return self.IsRefPathExtension() || self.IsSpanSlice();
        }

        public static bool IsRefPathExtension(this IMethodSymbol self)
        {
            if (!self.IsExtensionMethod)
                return false;

            return self.ReturnsRef() ?
                self.Is(RefListIndexer, RefPathItem, RefPathSkip) :
                self.IsRefListAsSpan();
        }

        public static bool IsRefListAsSpan(this IMethodSymbol self)
        {
            if (!self.IsExtensionMethod || !self.ReturnType.IsRefLikeType)
                return false;

            var p = self.Parameters.First();
            if (p.RefKind == RefKind.None || !p.Type.IsRefList())
                return false;

            if (!self.ReturnType.IsSpan(out var isReadOnly))
                return false;

            return (isReadOnly && p.RefKind == RefKind.In && self.Name == "AsReadOnlySpan") ||
                   (!isReadOnly && p.RefKind == RefKind.Ref && self.Name == "AsSpan");
        }

        public static bool IsSpanSlice(this IMethodSymbol self)
        {
            return self.ReturnType.IsRefLikeType && self.Name == "Slice" && self.ContainingType.IsSpan(out _);
        }

        public static bool IsNoCopyReturn(this IMethodSymbol self) => self.Is(NoCopyReturn);
        public static bool IsNonAllocatedResultRef(this IMethodSymbol self) => self.Is(NonAllocatedResult);
        public static bool IsRefListIndexer(this IMethodSymbol self) => self.Is(RefListIndexer);
        public static bool IsRefPathItem(this IMethodSymbol self) => self.Is(RefPathItem);
        public static bool IsRefPathSkip(this IMethodSymbol self) => self.Is(RefPathSkip);

        private static bool Is(this IMethodSymbol self, string attributeName)
        {
            return self.GetAttributes().Any(a => a.Is(attributeName));
        }

        private static bool Is(this IMethodSymbol self, string a1, string a2, string a3)
        {
            return self.GetAttributes().Any(a => a.Is(a1) || a.Is(a2) || a.Is(a3));
        }

        public static bool IsNoCopyType(this ITypeSymbol self) => self.Is(NoCopy);
        public static bool IsDealloc(this ITypeSymbol self) => self.Is(Dealloc);
        public static bool IsRefList(this ITypeSymbol self) => self.Is(RefList);
        public static bool IsUnmanagedRefList(this ITypeSymbol self) => self.IsUnmanagedType && self.IsRefList();
        public static bool IsDeallocOrRefList(this ITypeSymbol self) => self.Is(Dealloc, RefList);
        public static bool IsDynSized(this ITypeSymbol self)
        {
            if (self.Is(DynSized))
                return true;

            if (self.IsSpan(out _))
                return self is INamedTypeSymbol nt && nt.TryGetGenericArg(out var gt) && gt!.IsDynSized();

            return false;
        }

        public static bool IsSpan(this ITypeSymbol self, out bool isReadOnly)
        {
            isReadOnly = false;

            if (!self.IsRefLikeType)
                return false;

            if (self is not INamedTypeSymbol nt)
                return false;

            if (nt.ContainingNamespace.Name != "System")
                return false;

            switch (nt.Name)
            {
                case "Span":
                    isReadOnly = false;
                    return true;
                case "ReadOnlySpan":
                    isReadOnly = true;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsWrappedRef(this ITypeSymbol self) => self.IsSpan(out _);

        public static bool IsRefOrWrappedRef(this ILocalSymbol self) => self.IsRef || self.Type.IsWrappedRef();

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


        public static bool IsRefOrWrappedRef(this IParameterSymbol self)
        {
            return self.RefKind is RefKind.Ref or RefKind.In ||
                   self.Type.IsWrappedRef();
        }

        public static bool IsMut(this IParameterSymbol self)
        {
            if (self.Type.IsSpan(out var readOnly))
            {
                return !readOnly;
            }

            return self.RefKind == RefKind.Ref;
        }

        public static bool IsDynNoResize(this IParameterSymbol self) => self.Is(DynNoResize);

        private static bool Is(this IParameterSymbol self, string attributeName)
        {
            return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
        }
    }
}