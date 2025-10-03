using System;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.Util;

[Flags]
public enum CodeGenTraits
{
    None = 0,
    RefLike = 1 << 0,
    Unmanaged = 1 << 1,
    TempAlloc = 1 << 3,
}

public static class CodeGenTraitsExtensions
{
    public static CodeGenTraits GetCodeGenTraits(this ITypeSymbol self)
    {
        var result = CodeGenTraits.None;

        if (self.IsRefLikeType)
            result |= CodeGenTraits.RefLike;

        if (self.IsUnmanagedType)
            result |= CodeGenTraits.Unmanaged;

        if (self.IsTempAlloc())
            result |= CodeGenTraits.TempAlloc;

        return result;
    }

    public static RefListKinds ToRefListKinds(this CodeGenTraits self)
    {
        if (self.Has(CodeGenTraits.RefLike))
            return RefListKinds.None;

        var result = RefListKinds.None;

        if (self.Has(CodeGenTraits.Unmanaged) && !self.Has(CodeGenTraits.TempAlloc))
            result |= RefListKinds.RefList;

        if (self.Has(CodeGenTraits.Unmanaged))
            result |= RefListKinds.TempRefList;

        if (!self.Has(CodeGenTraits.TempAlloc))
            result |= RefListKinds.ManagedRefList;

        return result;
    }

    private static bool Has(this CodeGenTraits self, CodeGenTraits value) => (self & value) != CodeGenTraits.None;
}