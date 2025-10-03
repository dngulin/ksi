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

public static class RefListUtils
{
    public static RefListKinds GetKinds(bool unmanaged, bool temp)
    {
        var result = RefListKinds.None;

        if (unmanaged && !temp)
            result |= RefListKinds.RefList;

        if (unmanaged)
            result |= RefListKinds.TempRefList;

        if (!temp)
            result |= RefListKinds.ManagedRefList;

        return result;
    }
}

public static class RefListKindsExtensions
{
    public static void Emit(this RefListKinds self, string tpl, StringBuilder sb, string t)
    {
        if (self.Contains(RefListKinds.RefList))
            sb.AppendLine(string.Format(tpl, "RefList", t));

        if (self.Contains(RefListKinds.TempRefList))
            sb.AppendLine(string.Format(tpl, "TempRefList", t));

        if (self.Contains(RefListKinds.ManagedRefList))
            sb.AppendLine(string.Format(tpl, "ManagedRefList", t));
    }

    public static void Emit(this RefListKinds self, string deallocTpl, string nonDeallocTpl, StringBuilder sb, string t)
    {
        if (self.Contains(RefListKinds.RefList))
            sb.AppendLine(string.Format(deallocTpl, "RefList", t));

        if (self.Contains(RefListKinds.TempRefList))
            sb.AppendLine(string.Format(nonDeallocTpl, "TempRefList", t));

        if (self.Contains(RefListKinds.ManagedRefList))
            sb.AppendLine(string.Format(nonDeallocTpl, "ManagedRefList", t));
    }

    private static bool Contains(this RefListKinds self, RefListKinds value) => (self & value) != RefListKinds.None;
}