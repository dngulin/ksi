using System;
using System.Buffers.Binary;

namespace Ksi.Serialization
{
    /// <summary>
    /// Provides extension methods for <see cref="Span{T}"/> to write binary data by prepending it to the end of the span.
    /// </summary>
    public static class SpanWriteExtensions
    {
        /// <summary>
        /// Prepends a <see cref="byte"/> to the end of the span and shrinks the span from the end by 1.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, byte value)
        {
            span[^1] = value;
            span = span[..^1];
        }

        /// <summary>
        /// Prepends an <see cref="sbyte"/> to the end of the span and shrinks the span from the end by 1.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, sbyte value)
        {
            span[^1] = (byte)value;
            span = span[..^1];
        }

        /// <summary>
        /// Prepends a <see cref="ushort"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span[^sizeof(ushort)..], value);
            span = span[..^sizeof(ushort)];
        }

        /// <summary>
        /// Prepends a <see cref="short"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span[^sizeof(short)..], value);
            span = span[..^sizeof(short)];
        }

        /// <summary>
        /// Prepends a <see cref="uint"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[^sizeof(uint)..], value);
            span = span[..^sizeof(uint)];
        }

        /// <summary>
        /// Prepends an <see cref="int"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[^sizeof(int)..], value);
            span = span[..^sizeof(int)];
        }

        /// <summary>
        /// Prepends a <see cref="float"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, float value)
        {
            span.Prepend(BitConverter.SingleToInt32Bits(value));
        }

        /// <summary>
        /// Prepends a <see cref="ulong"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span[^sizeof(ulong)..], value);
            span = span[..^sizeof(ulong)];
        }

        /// <summary>
        /// Prepends a <see cref="long"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(span[^sizeof(long)..], value);
            span = span[..^sizeof(long)];
        }

        /// <summary>
        /// Prepends a <see cref="double"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, double value)
        {
            span.Prepend(BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Prepends a <see cref="ReadOnlySpan{T}"/> of bytes to the end of the span and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Prepend(this ref Span<byte> span, ReadOnlySpan<byte> value)
        {
            value.CopyTo(span[^value.Length..]);
            span = span[..^value.Length];
        }

        /// <summary>
        /// Prepends a length prefix to the end of the span and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="len">The length to write.</param>
        /// <param name="lps">The size of the length prefix used.</param>
        public static void PrependLenPrefix(this ref Span<byte> span, uint len, out LenPrefixSize lps)
        {
            lps = ValueQualifier.GetLenPrefix(len);
            switch (lps)
            {
                case LenPrefixSize._0:
                    return;
                case LenPrefixSize._8:
                    span.Prepend((byte)len);
                    return;
                case LenPrefixSize._16:
                    span.Prepend((ushort)len);
                    return;
                case LenPrefixSize._32:
                    span.Prepend(len);
                    return;
                default:
                    throw new Exception($"Unreachable! Invalid LenPrefixSize estimation: {(byte)lps}");
            }
        }
    }
}