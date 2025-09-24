using System.Text;

namespace Ksi.Roslyn
{
    public static class EmitUtils
    {
        public static void EmitRefListMethods(string tpl, StringBuilder sb, string typeName, bool unmanaged, bool temp)
        {
            if (!temp && unmanaged)
                sb.AppendLine(string.Format(tpl, "RefList", typeName));

            if (unmanaged)
                sb.AppendLine(string.Format(tpl, "TempRefList", typeName));

            if (!temp)
                sb.AppendLine(string.Format(tpl, "ManagedRefList", typeName));
        }
    }
}