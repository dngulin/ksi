using System;

namespace Ksi.Roslyn.Util;

[Flags]
public enum RefListKinds
{
    None = 0,
    RefList = 1 << 0,
    TempRefList = 1 << 1,
    ManagedRefList = 1 << 2,
}

public static class RefListKindsExtensions
{
    public static void Emit(this RefListKinds self, in AppendScope scope, string tpl, string t)
    {
        if (self.Has(RefListKinds.RefList))
            scope.AppendLine(string.Format(tpl, "RefList", t));

        if (self.Has(RefListKinds.TempRefList))
            scope.AppendLine(string.Format(tpl, "TempRefList", t));

        if (self.Has(RefListKinds.ManagedRefList))
            scope.AppendLine(string.Format(tpl, "ManagedRefList", t));
    }

    public static void Emit(this RefListKinds self, in AppendScope scope, string mainTpl, string otherTpl, string t)
    {
        if (self.Has(RefListKinds.RefList))
            scope.AppendLine(string.Format(mainTpl, "RefList", t));

        if (self.Has(RefListKinds.TempRefList))
            scope.AppendLine(string.Format(otherTpl, "TempRefList", t));

        if (self.Has(RefListKinds.ManagedRefList))
            scope.AppendLine(string.Format(otherTpl, "ManagedRefList", t));
    }

    private static bool Has(this RefListKinds self, RefListKinds value) => (self & value) != RefListKinds.None;
}