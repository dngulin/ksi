using System;

namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the size of a primitive value in bits.
    /// </summary>
    public enum PrimitiveSize : byte
    {
        /// <summary>
        /// 8-bit primitive (1 byte).
        /// </summary>
        _8,

        /// <summary>
        /// 16-bit primitive (2 bytes).
        /// </summary>
        _16,

        /// <summary>
        /// 32-bit primitive (4 bytes).
        /// </summary>
        _32,

        /// <summary>
        /// 64-bit primitive (8 bytes).
        /// </summary>
        _64,
    }

    /// <summary>
    /// Provides extension methods for <see cref="PrimitiveSize"/>.
    /// </summary>
    public static class PrimitiveSizeExtensions
    {
        /// <summary>
        /// Gets the size of the primitive in bytes.
        /// </summary>
        /// <param name="size">The <see cref="PrimitiveSize"/>.</param>
        /// <returns>The size in bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid <see cref="PrimitiveSize"/> is provided.</exception>
        public static int InBytes(this PrimitiveSize size)
        {
            return size switch
            {
                PrimitiveSize._8 => 1,
                PrimitiveSize._16 => 2,
                PrimitiveSize._32 => 4,
                PrimitiveSize._64 => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }
}