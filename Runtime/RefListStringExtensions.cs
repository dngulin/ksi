using System.Text;

namespace DnDev
{
    public static class RefListStringExtensions
    {
        public static void AppendUtf8String(this ref RefList<byte> list, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }


            var pos = list.Count();
            list.AppendDefault(Encoding.UTF8.GetByteCount(value));
            Encoding.UTF8.GetBytes(value, 0, value.Length, list.Buffer, pos);
        }

        public static void AppendAsciiString(this ref RefList<byte> list, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var pos = list.Count();
            list.AppendDefault(value.Length);
            Encoding.ASCII.GetBytes(value, 0, value.Length, list.Buffer, pos);
        }

        public static string ToStringUtf8(this in RefList<byte> list)
        {
            return list.Count == 0 ? "" : Encoding.UTF8.GetString(list.Buffer, 0, list.Count);
        }

        public static string ToStringAscii(this in RefList<byte> list)
        {
            return list.Count == 0 ? "" : Encoding.ASCII.GetString(list.Buffer, 0, list.Count);
        }
    }
}