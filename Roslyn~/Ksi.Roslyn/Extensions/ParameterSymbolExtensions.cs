using System.Linq;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.Extensions;

public static class ParameterSymbolExtensions
{
    public static bool IsRef(this IParameterSymbol self) => self.RefKind is RefKind.Ref or RefKind.In;

    public static bool IsRefOrWrappedRef(this IParameterSymbol self) => self.IsRef() || self.Type.IsWrappedRef();

    public static bool IsMut(this IParameterSymbol self)
    {
        if (self.Type.IsWrappedRef(out var isMut))
            return isMut;

        return self.RefKind == RefKind.Ref;
    }

    public static bool IsDynNoResize(this IParameterSymbol self) => self.Is(SymbolNames.DynNoResize);
    public static bool IsKsiQueryParam(this IParameterSymbol self) => self.Is(SymbolNames.KsiQueryParam);

    private static bool Is(this IParameterSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
    }
}