using System;
using System.Buffers.Binary;

namespace Ksi.Serialization
{
    public static class SpanReadExtensions
    {
        public static byte ReadByte(this ref ReadOnlySpan<byte> span)
        {
            var value = span[0];
            span = span[1..];
            return value;
        }

        public static sbyte ReadSByte(this ref ReadOnlySpan<byte> span)
        {
            var value = (sbyte)span[0];
            span = span[1..];
            return value;
        }

        public static ushort ReadUInt16(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(span);
            span = span[sizeof(ushort)..];
            return value;
        }

        public static short ReadInt16(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(span);
            span = span[sizeof(short)..];
            return value;
        }

        public static uint ReadUInt32(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(span);
            span = span[sizeof(uint)..];
            return value;
        }

        public static int ReadInt32(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(span);
            span = span[sizeof(int)..];
            return value;
        }

        public static float ReadSingle(this ref ReadOnlySpan<byte> span)
        {
            return BitConverter.Int32BitsToSingle(span.ReadInt32());
        }

        public static ulong ReadUInt64(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(span);
            span = span[sizeof(ulong)..];
            return value;
        }

        public static long ReadInt64(this ref ReadOnlySpan<byte> span)
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(span);
            span = span[sizeof(long)..];
            return value;
        }

        public static double ReadDouble(this ref ReadOnlySpan<byte> span)
        {
            return BitConverter.Int64BitsToDouble(span.ReadInt64());
        }

        public static void Read(this ref ReadOnlySpan<byte> span, Span<byte> value)
        {
            span[..value.Length].CopyTo(value);
            span = span[value.Length..];
        }

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