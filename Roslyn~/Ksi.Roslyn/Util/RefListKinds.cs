using System;
using System.Text;

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
    public static void Emit(this RefListKinds self, string tpl, StringBuilder sb, string t)
    {
        if (self.Has(RefListKinds.RefList))
            sb.AppendLine(string.Format(tpl, "RefList", t));

        if (self.Has(RefListKinds.TempRefList))
            sb.AppendLine(string.Format(tpl, "TempRefList", t));

        if (self.Has(RefListKinds.ManagedRefList))
            sb.AppendLine(string.Format(tpl, "ManagedRefList", t));
    }

    public static void Emit(this RefListKinds self, string deallocTpl, string nonDeallocTpl, StringBuilder sb, string t)
    {
        if (self.Has(RefListKinds.RefList))
            sb.AppendLine(string.Format(deallocTpl, "RefList", t));

        if (self.Has(RefListKinds.TempRefList))
            sb.AppendLine(string.Format(nonDeallocTpl, "TempRefList", t));

        if (self.Has(RefListKinds.ManagedRefList))
            sb.AppendLine(string.Format(nonDeallocTpl, "ManagedRefList", t));
    }

    private static bool Has(this RefListKinds self, RefListKinds value) => (self & value) != RefListKinds.None;
}