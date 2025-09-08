using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

public readonly struct RefVarInfo(IVariableDeclaratorOperation declarator, RefVarKind kind, IOperation producer)
{
    public readonly IVariableDeclaratorOperation Declarator = declarator;
    public readonly RefVarKind Kind = kind;
    public readonly IOperation Producer = producer;
}

public enum RefVarKind
{
    LocalRef,
    IteratorRef
}

public static class RefVarInfoExtensions
{
    public static bool ReferencesDynSizeInstance(this in RefVarInfo self)
    {
        var p = self.Producer;

        return self.Kind switch
        {
            RefVarKind.LocalRef => p.ReferencesDynSizeInstance(),
            RefVarKind.IteratorRef => p.IsRefListIterator(out _),
            _ => false
        };
    }

    public static RefPath GetRefPath(this in RefVarInfo self)
    {
        return self.Kind switch
        {
            RefVarKind.LocalRef => self.Producer.ToRefPath(),
            RefVarKind.IteratorRef => self.Producer.IsRefListIterator(out var src) ? src!.ToRefPath() : RefPath.Empty,
            _ => RefPath.Empty
        };
    }
}