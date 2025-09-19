using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn;

public readonly struct RefVarInfo(ILocalSymbol symbol, RefVarKind kind, IOperation producer)
{
    public readonly ILocalSymbol Symbol = symbol;
    public readonly RefVarKind Kind = kind;
    public readonly IOperation Producer = producer;
}

public enum RefVarKind
{
    LocalSymbolRef,
    IteratorItemRef
}

public static class RefVarInfoExtensions
{
    public static bool ReferencesDynSizeInstance(this in RefVarInfo self)
    {
        var p = self.Producer;

        return self.Kind switch
        {
            RefVarKind.LocalSymbolRef => p.ReferencesDynSizeInstance(),
            RefVarKind.IteratorItemRef => p.IsRefListIterator(out _),
            _ => false
        };
    }

    public static RefPath GetRefPath(this in RefVarInfo self)
    {
        var p = self.Producer;
        var t = self.Symbol.Type;

        return self.Kind switch
        {
            RefVarKind.LocalSymbolRef => p.ToRefPath(),
            RefVarKind.IteratorItemRef => p.IsRefListIterator(out var src) ? src!.ToRefPath(t) : RefPath.Empty,
            _ => RefPath.Empty
        };
    }
}