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

    public static bool IsDynSized(this ITypeSymbol self)
    {
        if (self.Is(SymbolNames.DynSized))
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

    public static bool IsNoCopyType(this ITypeSymbol self) => self.Is(SymbolNames.NoCopy);
    public static bool IsDealloc(this ITypeSymbol self) => self.Is(SymbolNames.Dealloc);
    public static bool IsRefList(this ITypeSymbol self) => self.Is(SymbolNames.RefList);
    public static bool IsUnmanagedRefList(this ITypeSymbol self) => self.IsUnmanagedType && self.IsRefList();
    public static bool IsDeallocOrRefList(this ITypeSymbol self) => self.Is(SymbolNames.Dealloc, SymbolNames.RefList);

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
}