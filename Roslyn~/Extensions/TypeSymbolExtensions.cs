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

    public static bool IsSpan(this INamedTypeSymbol self) => self.IsRefLike("System", "Span");
    private static bool IsReadOnlySpan(this INamedTypeSymbol self) => self.IsRefLike("System", "ReadOnlySpan");

    private static bool IsRefLike(this INamedTypeSymbol self, string ns, string name)
    {
        return self.IsRefLikeType && self.ContainingNamespace.Name == ns && self.Name == name;
    }

    public static bool IsWrappedRef(this ITypeSymbol self, out bool isMut) => self.IsSpanOrReadonlySpan(out isMut);
    public static bool IsWrappedRef(this ITypeSymbol self) => self.IsWrappedRef(out _);

    public static bool IsExplicitCopy(this ITypeSymbol self) => self.Is(SymbolNames.ExplicitCopy);
    public static bool IsDealloc(this ITypeSymbol self) => self.Is(SymbolNames.Dealloc);
    public static bool IsRefList(this ITypeSymbol self) => self.Is(SymbolNames.RefList);
    public static bool IsDynSized(this ITypeSymbol self) => self.Is(SymbolNames.DynSized);
    public static bool IsUnmanagedRefList(this ITypeSymbol self) => self.IsUnmanagedType && self.IsRefList();

    private static bool Is(this ITypeSymbol self, string attributeName)
    {
        if (self.TypeKind != TypeKind.Struct)
            return false;

        return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
    }
}