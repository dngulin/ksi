using System.Text;

namespace Ksi.Roslyn
{
    public static class EmitUtils
    {
        public const string RootNamespace = "Ksi";

        public static void EmitRefListMethods(string tpl, StringBuilder sb, string typeName, bool unmanaged)
        {
            sb.AppendLine(string.Format(tpl, "RefList", typeName));
            if (!unmanaged)
                return;

            sb.AppendLine(string.Format(tpl, "NativeRefList", typeName));
            sb.AppendLine(string.Format(tpl, "TempRefList", typeName));
        }
    }
}