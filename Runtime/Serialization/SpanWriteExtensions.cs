using System;
using System.Buffers.Binary;

namespace Ksi.Serialization
{
    public static class SpanWriteExtensions
    {
        public static int Prepend(this ref Span<byte> span, byte value)
        {
            span[^1] = value;
            span = span[..^1];
            return 1;
        }

        public static int Prepend(this ref Span<byte> span, sbyte value)
        {
            span[^1] = (byte)value;
            span = span[..^1];
            return 1;
        }

        public static int Prepend(this ref Span<byte> span, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span[^sizeof(ushort)..], value);
            span = span[..^sizeof(ushort)];
            return sizeof(ushort);
        }

        public static int Prepend(this ref Span<byte> span, short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span[^sizeof(short)..], value);
            span = span[..^sizeof(short)];
            return sizeof(short);
        }

        public static int Prepend(this ref Span<byte> span, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[^sizeof(uint)..], value);
            span = span[..^sizeof(uint)];
            return sizeof(uint);
        }

        public static int Prepend(this ref Span<byte> span, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[^sizeof(int)..], value);
            span = span[..^sizeof(int)];
            return sizeof(int);
        }

        public static int Prepend(this ref Span<byte> span, float value)
        {
            return span.Prepend(BitConverter.SingleToInt32Bits(value));
        }

        public static int Prepend(this ref Span<byte> span, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span[^sizeof(ulong)..], value);
            span = span[..^sizeof(ulong)];
            return sizeof(ulong);
        }

        public static int Prepend(this ref Span<byte> span, long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(span[^sizeof(long)..], value);
            span = span[..^sizeof(long)];
            return sizeof(long);
        }

        public static int Prepend(this ref Span<byte> span, double value)
        {
            return span.Prepend(BitConverter.DoubleToInt64Bits(value));
        }

        public static int Prepend(this ref Span<byte> span, ReadOnlySpan<byte> value)
        {
            value.CopyTo(span[^value.Length..]);
            span = span[..^value.Length];
            return value.Length;
        }

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