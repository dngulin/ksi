namespace Ksi.Serialization
{
    public static class KsiSerializedSize
    {
        private static int GetLenPrefixSize(uint len)
        {
            return len switch
            {
                0 => 0,
                <= byte.MaxValue => sizeof(byte),
                <= ushort.MaxValue => sizeof(ushort),
                _ => sizeof(uint),
            };
        }

        public static int Primitive(int size)
        {
            return ValueQualifier.PackedSize + size;
        }

        public static int RepeatedPrimitive(int itemSize, int count)
        {
            return Struct(itemSize * count);
        }

        public static int Struct(int size)
        {
            return ValueQualifier.PackedSize +
                   GetLenPrefixSize((uint) size) +
                   size;
        }

        public static int RepeatedStruct(int totalSize, int count)
        {
            return ValueQualifier.PackedSize +
                   GetLenPrefixSize((uint) totalSize) +
                   GetLenPrefixSize((uint) count) +
                   totalSize;
        }
    }
}