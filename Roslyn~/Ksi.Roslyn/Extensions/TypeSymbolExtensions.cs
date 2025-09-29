using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.Extensions;

public static class TypeSymbolExtensions
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

    public static bool IsDynSizedOrWrapsDynSized(this ITypeSymbol self)
    {
        return self.Is(SymbolNames.DynSized) || self.WrapsDynSized();
    }

    public static bool WrapsDynSized(this ITypeSymbol self)
    {
        if (!self.IsRefLikeType || self is not INamedTypeSymbol { IsGenericType: true } nt)
            return false;

        if (nt.TypeArguments.Length != 1 || nt.TypeArguments.First() is not INamedTypeSymbol gt)
            return false;

        return nt.IsWrappedRef() && gt.IsDynSizedOrWrapsDynSized();
    }

    public static bool IsNotSupportedGenericOverExplicitCopy(this INamedTypeSymbol self, out ITypeSymbol? t)
    {
        if (!self.IsGenericType || self.IsSupportedGenericType())
        {
            t = null;
            return false;
        }

        foreach (var arg in self.TypeArguments)
        {
            if (arg is not INamedTypeSymbol namedArg)
                continue;

            if (namedArg.IsExplicitCopy())
            {
                t = namedArg;
                return true;
            }

            if (namedArg.IsNotSupportedGenericOverExplicitCopy(out t))
                return true;
        }

        t = null;
        return false;
    }

    public static bool IsSupportedGenericType(this INamedTypeSymbol self)
    {
        return self is { IsGenericType: true, TypeArguments.Length: 1 } &&
               (self.IsWrappedRef() || self.IsRefList() || self.IsExclusiveAccess());
    }

    public static bool IsSpanOrReadonlySpan(this ITypeSymbol self, out bool isMut)
    {
        isMut = false;

        if (!self.IsRefLikeType || self is not INamedTypeSymbol nt)
            return false;

        if (nt.IsSpan())
        {
            isMut = true;
            return true;
        }

        return nt.IsReadOnlySpan();
    }

    public static bool IsSpanOrReadonlySpan(this ITypeSymbol self) => self.IsSpanOrReadonlySpan(out _);

    public static bool IsSpan(this INamedTypeSymbol self) => self.IsRefLikeGeneric("System", "Span");
    private static bool IsReadOnlySpan(this INamedTypeSymbol self) => self.IsRefLikeGeneric("System", "ReadOnlySpan");

    public static bool IsAccessScope(this ITypeSymbol self) => self.IsAccessScope(out _);

    public static bool IsAccessScope(this ITypeSymbol self, out bool isMut)
    {
        isMut = false;

        if (!self.IsRefLikeType || self is not INamedTypeSymbol nt)
            return false;

        if (nt.IsMutableAccessScope())
        {
            isMut = true;
            return true;
        }

        return nt.IsReadOnlyAccessScope();
    }

    public static bool IsMutableAccessScope(this INamedTypeSymbol self) => self.IsRefLikeGeneric("Ksi", "MutableAccessScope");
    private static bool IsReadOnlyAccessScope(this INamedTypeSymbol self) => self.IsRefLikeGeneric("Ksi", "AccessScope");

    private static bool IsRefLikeGeneric(this INamedTypeSymbol self, string ns, string name)
    {
        return self is { IsRefLikeType: true, IsGenericType: true, TypeArguments.Length: 1 }
               && self.ContainingNamespace.Name == ns && self.Name == name;
    }

    public static bool IsExclusiveAccess(this INamedTypeSymbol self) => self.IsGenericClass("Ksi", "ExclusiveAccess");

    private static bool IsGenericClass(this INamedTypeSymbol self, string ns, string name)
    {
        return self is { IsGenericType: true, TypeArguments.Length: 1, TypeKind: TypeKind.Class }
               && self.ContainingNamespace.Name == ns && self.Name == name;
    }

    public static bool IsWrappedRef(this ITypeSymbol self, out bool isMut)
    {
        return self.IsSpanOrReadonlySpan(out isMut) || self.IsAccessScope(out isMut);
    }

    public static bool IsWrappedRef(this ITypeSymbol self) => self.IsWrappedRef(out _);

    public static bool IsExplicitCopy(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.ExplicitCopy);
    }

    public static bool IsDealloc(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.Dealloc);
    }

    public static bool IsDeallocOrRefListOverDealloc(this ITypeSymbol self)
    {
        if (self.IsDealloc())
            return true;

        if (!self.IsRefList())
            return false;

        return self is INamedTypeSymbol nt && nt.TypeArguments.First().IsDealloc();
    }

    public static bool IsRefList(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.RefList);
    public static bool IsDynSized(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.DynSized);
    public static bool IsTemp(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.Temp);

    private static bool Is(this ITypeSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
    }

    public static bool IsStruct(this ITypeSymbol self) => self.TypeKind == TypeKind.Struct;
    public static bool IsStructOrTypeParameter(this ITypeSymbol self)
    {
        return self.TypeKind switch
        {
            TypeKind.Struct => true,
            TypeKind.TypeParameter => true,
            _ => false
        };
    }
}