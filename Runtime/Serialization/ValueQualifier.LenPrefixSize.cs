using System;

namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the size of the length prefix in bytes.
    /// </summary>
    public enum LenPrefixSize : byte
    {
        /// <summary>
        /// No length prefix (0 bytes).
        /// </summary>
        _0,

        /// <summary>
        /// 8-bit length prefix (1 byte).
        /// </summary>
        _8,

        /// <summary>
        /// 16-bit length prefix (2 bytes).
        /// </summary>
        _16,

        /// <summary>
        /// 32-bit length prefix (4 bytes).
        /// </summary>
        _32,
    }

    /// <summary>
    /// Provides extension methods for <see cref="LenPrefixSize"/>.
    /// </summary>
    public static class LenPrefixSizeExtensions
    {
        /// <summary>
        /// Gets the size of the length prefix in bytes.
        /// </summary>
        /// <param name="size">The <see cref="LenPrefixSize"/>.</param>
        /// <returns>The size in bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid <see cref="LenPrefixSize"/> is provided.</exception>
        public static int InBytes(this LenPrefixSize size)
        {
            return size switch
            {
                LenPrefixSize._0 => 0,
                LenPrefixSize._8 => 1,
                LenPrefixSize._16 => 2,
                LenPrefixSize._32 => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }
}