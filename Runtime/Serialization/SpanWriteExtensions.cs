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
        /// <returns>The number of bytes written (1).</returns>
        public static int Prepend(this ref Span<byte> span, byte value)
        {
            span[^1] = value;
            span = span[..^1];
            return 1;
        }

        /// <summary>
        /// Prepends an <see cref="sbyte"/> to the end of the span and shrinks the span from the end by 1.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written (1).</returns>
        public static int Prepend(this ref Span<byte> span, sbyte value)
        {
            span[^1] = (byte)value;
            span = span[..^1];
            return 1;
        }

        /// <summary>
        /// Prepends a <see cref="ushort"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span[^sizeof(ushort)..], value);
            span = span[..^sizeof(ushort)];
            return sizeof(ushort);
        }

        /// <summary>
        /// Prepends a <see cref="short"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span[^sizeof(short)..], value);
            span = span[..^sizeof(short)];
            return sizeof(short);
        }

        /// <summary>
        /// Prepends a <see cref="uint"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[^sizeof(uint)..], value);
            span = span[..^sizeof(uint)];
            return sizeof(uint);
        }

        /// <summary>
        /// Prepends an <see cref="int"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[^sizeof(int)..], value);
            span = span[..^sizeof(int)];
            return sizeof(int);
        }

        /// <summary>
        /// Prepends a <see cref="float"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, float value)
        {
            return span.Prepend(BitConverter.SingleToInt32Bits(value));
        }

        /// <summary>
        /// Prepends a <see cref="ulong"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span[^sizeof(ulong)..], value);
            span = span[..^sizeof(ulong)];
            return sizeof(ulong);
        }

        /// <summary>
        /// Prepends a <see cref="long"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(span[^sizeof(long)..], value);
            span = span[..^sizeof(long)];
            return sizeof(long);
        }

        /// <summary>
        /// Prepends a <see cref="double"/> to the end of the span in little-endian format and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, double value)
        {
            return span.Prepend(BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Prepends a <see cref="ReadOnlySpan{T}"/> of bytes to the end of the span and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Prepend(this ref Span<byte> span, ReadOnlySpan<byte> value)
        {
            value.CopyTo(span[^value.Length..]);
            span = span[..^value.Length];
            return value.Length;
        }

        /// <summary>
        /// Prepends a length prefix to the end of the span and shrinks the span from the end.
        /// </summary>
        /// <param name="span">The span to write to.</param>
        /// <param name="len">The length to write.</param>
        /// <param name="lps">The size of the length prefix used.</param>
        /// <returns>The number of bytes written.</returns>
        public static int PrependLenPrefix(this ref Span<byte> span, uint len, out LenPrefixSize lps)
        {
            lps = ValueQualifier.GetLenPrefix(len);
            switch (lps)
            {
                case LenPrefixSize._0:
                    return 0;
                case LenPrefixSize._8:
                    return span.Prepend((byte)len);
                case LenPrefixSize._16:
                    return span.Prepend((ushort)len);
                case LenPrefixSize._32:
                    return span.Prepend(len);
                default:
                    throw new Exception($"Unreachable! Invalid LenPrefixSize estimation: {(byte)lps}");
            }
        }
    }
}