using System;
using System.Buffers.Binary;

namespace Ksi.Serialization
{
    /// <summary>
    /// Provides extension methods for <see cref="ReadOnlySpan{T}"/> to read binary data.
    /// </summary>
    public static class SpanReadExtensions
    {
        /// <summary>
        /// Reads a <see cref="byte"/> from the start of the span and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static byte ReadByte(this ref ReadOnlySpan<byte> span)
        {
            var value = span[0];
            span = span[1..];
            return value;
        }

        /// <summary>
        /// Reads an <see cref="sbyte"/> from the start of the span and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static sbyte ReadSByte(this ref ReadOnlySpan<byte> span)
        {
            var value = (sbyte)span[0];
            span = span[1..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static ushort ReadUInt16(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(span);
            span = span[sizeof(ushort)..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="short"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static short ReadInt16(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(span);
            span = span[sizeof(short)..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static uint ReadUInt32(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(span);
            span = span[sizeof(uint)..];
            return value;
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static int ReadInt32(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(span);
            span = span[sizeof(int)..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static float ReadSingle(this ref ReadOnlySpan<byte> span)
        {
            return BitConverter.Int32BitsToSingle(span.ReadInt32());
        }

        /// <summary>
        /// Reads a <see cref="ulong"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static ulong ReadUInt64(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(span);
            span = span[sizeof(ulong)..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="long"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static long ReadInt64(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(span);
            span = span[sizeof(long)..];
            return value;
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the start of the span in little-endian format and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <returns>The value read.</returns>
        public static double ReadDouble(this ref ReadOnlySpan<byte> span)
        {
            return BitConverter.Int64BitsToDouble(span.ReadInt64());
        }

        /// <summary>
        /// Copies data from the start of the source span to the destination span and shrinks the source span from the start.
        /// </summary>
        /// <param name="span">The source span to read from.</param>
        /// <param name="value">The destination span to write to.</param>
        public static void Read(this ref ReadOnlySpan<byte> span, Span<byte> value)
        {
            span[..value.Length].CopyTo(value);
            span = span[value.Length..];
        }

        /// <summary>
        /// Reads a length prefix from the start of the span and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        /// <param name="lps">The size of the length prefix to read.</param>
        /// <returns>The length value read.</returns>
        public static uint ReadLenPrefix(this ref ReadOnlySpan<byte> span, LenPrefixSize lps)
        {
            return lps switch
            {
                LenPrefixSize._0 => 0,
                LenPrefixSize._8 => span.ReadByte(),
                LenPrefixSize._16 => span.ReadUInt16(),
                LenPrefixSize._32 => span.ReadUInt32(),
                _ => throw new InvalidOperationException()
            };
        }

        /// <summary>
        /// Skips a value in the span based on its qualifier and shrinks the span from the start.
        /// </summary>
        /// <param name="span">The span to skip in.</param>
        /// <param name="q">The qualifier of the value to skip.</param>
        public static void Skip(this ref ReadOnlySpan<byte> span, ValueQualifier q)
        {
            var len = q.Kind switch
            {
                ValueKind.Primitive => (uint) q.PrimitiveSize.InBytes(),
                _ => span.ReadLenPrefix(q.LenPrefixSize),
            };

            span = span[(int)len..];
        }
    }
}