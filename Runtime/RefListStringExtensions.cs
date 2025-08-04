using System.Text;

namespace Frog.Collections
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
            Encoding.UTF8.GetBytes(value, 0, value.Length, list.ItemArray, pos);
        }

        public static void AppendAsciiString(this ref RefList<byte> list, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var pos = list.Count();
            list.AppendDefault(value.Length);
            Encoding.ASCII.GetBytes(value, 0, value.Length, list.ItemArray, pos);
        }

        public static string ToStringUtf8(this in RefList<byte> list)
        {
            return list.ItemCount == 0 ? "" : Encoding.UTF8.GetString(list.ItemArray, 0, list.ItemCount);
        }

        public static string ToStringAscii(this in RefList<byte> list)
        {
            return list.ItemCount == 0 ? "" : Encoding.ASCII.GetString(list.ItemArray, 0, list.ItemCount);
        }
    }
}