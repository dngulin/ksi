using System;

namespace Ksi.Serialization
{
    public enum PrimitiveSize : byte
    {
        _8,
        _16,
        _32,
        _64,
    }

    public static class PrimitiveSizeExtensions
    {
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