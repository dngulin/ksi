using System.Text;

namespace Ksi.Roslyn
{
    public static class EmitUtils
    {
        public static void EmitRefListMethods(string tpl, StringBuilder sb, string typeName, bool unmanaged)
        {
            if (unmanaged)
            {
                sb.AppendLine(string.Format(tpl, "RefList", typeName));
                sb.AppendLine(string.Format(tpl, "TempRefList", typeName));
            }

            sb.AppendLine(string.Format(tpl, "ManagedRefList", typeName));
        }
    }
}