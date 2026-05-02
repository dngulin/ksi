namespace Ksi.Serialization
{
    /// <summary>
    /// Utility class for calculating the size of serialized data.
    /// </summary>
    public static class KsiSerializedSize
    {
        /// <summary>
        /// Gets the size of the length prefix for a given length.
        /// </summary>
        /// <param name="len">The length to get the prefix size for.</param>
        /// <returns>The size of the length prefix in bytes.</returns>
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

        /// <summary>
        /// Calculates the serialized size of a primitive value.
        /// </summary>
        /// <param name="size">The size of the primitive value in bytes.</param>
        /// <returns>The total serialized size including the value qualifier.</returns>
        public static int Primitive(int size)
        {
            return ValueQualifier.PackedSize + size;
        }

        /// <summary>
        /// Calculates the serialized size of a repeated primitive value.
        /// </summary>
        /// <param name="itemSize">The size of a single item in bytes.</param>
        /// <param name="count">The number of items.</param>
        /// <returns>The total serialized size.</returns>
        public static int RepeatedPrimitive(int itemSize, int count)
        {
            return Struct(itemSize * count);
        }

        /// <summary>
        /// Calculates the serialized size of a struct.
        /// </summary>
        /// <param name="size">The size of the struct content in bytes.</param>
        /// <returns>The total serialized size including the value qualifier and length prefix.</returns>
        public static int Struct(int size)
        {
            return ValueQualifier.PackedSize +
                   GetLenPrefixSize((uint) size) +
                   size;
        }

        /// <summary>
        /// Calculates the serialized size of a repeated struct.
        /// </summary>
        /// <param name="totalSize">The total size of all struct items in bytes.</param>
        /// <param name="count">The number of items.</param>
        /// <returns>The total serialized size including value qualifier, length prefix, and count prefix.</returns>
        public static int RepeatedStruct(int totalSize, int count)
        {
            return ValueQualifier.PackedSize +
                   GetLenPrefixSize((uint) totalSize) +
                   GetLenPrefixSize((uint) count) +
                   totalSize;
        }
    }
}