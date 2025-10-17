using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.Extensions;

public static class MethodSymbolExtensions
{
    public static bool ReturnsRef(this IMethodSymbol self) => self.RefKind is RefKind.Ref or RefKind.RefReadOnly;

    public static bool ReturnsRefOrSpan(this IMethodSymbol self)
    {
        return self.ReturnsRef() || self.ReturnType.IsSpanOrReadonlySpan();
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
            self.Is(SymbolNames.RefListIndexer, SymbolNames.RefPath) :
            self.IsRefListAsSpan();
    }

    public static bool IsRefListAsSpan(this IMethodSymbol self)
    {
        if (!self.IsExtensionMethod || !self.ReturnType.IsRefLikeType)
            return false;

        var p = self.Parameters.First();
        if (p.RefKind == RefKind.None || !p.Type.IsRefList())
            return false;

        if (!self.ReturnType.IsSpanOrReadonlySpan(out var isMut))
            return false;

        return (!isMut && p.RefKind == RefKind.In && self.Name == "AsReadOnlySpan") ||
               (isMut && p.RefKind == RefKind.Ref && self.Name == "AsSpan");
    }

    public static bool IsSpanSlice(this IMethodSymbol self)
    {
        return self.ReturnType.IsRefLikeType && self.Name == "Slice" && self.ContainingType.IsSpanOrReadonlySpan();
    }

    public static bool IsNonAllocatedResultRef(this IMethodSymbol self) => self.Is(SymbolNames.NonAllocatedResult);
    public static bool IsRefListIndexer(this IMethodSymbol self) => self.Is(SymbolNames.RefListIndexer);
    public static bool IsKsiQuery(this IMethodSymbol self) => self.Is(SymbolNames.KsiQuery);
    public static bool IsRefPath(this IMethodSymbol self, out ImmutableArray<string?> segments)
    {
        foreach (var attr in self.GetAttributes().Where(attr => attr.Is(SymbolNames.RefPath)))
        {
            segments = attr.GetRefPathSegments();
            return true;
        }

        segments = ImmutableArray<string?>.Empty;
        return false;
    }

    private static ImmutableArray<string?> GetRefPathSegments(this AttributeData self)
    {
        if (self.ConstructorArguments.Length == 0)
            return ImmutableArray<string?>.Empty;

        var first = self.ConstructorArguments.First();
        if (first.IsNull)
            return ImmutableArray<string?>.Empty;

        return first.Values.Select(v => (string?)v.Value).ToImmutableArray();
    }

    private static bool Is(this IMethodSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(a => a.Is(attributeName));
    }

    private static bool Is(this IMethodSymbol self, string a1, string a2)
    {
        return self.GetAttributes().Any(a => a.Is(a1) || a.Is(a2));
    }
}