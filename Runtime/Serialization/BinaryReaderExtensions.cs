using System;
using System.IO;

namespace Ksi.Serialization
{
    public static class BinaryReaderExtensions
    {
        public static uint ReadLenPrefix(this BinaryReader br, LenPrefixSize lps)
        {
            return lps switch
            {
                LenPrefixSize._0 => 0,
                LenPrefixSize._8 => br.ReadByte(),
                LenPrefixSize._16 => br.ReadUInt16(),
                LenPrefixSize._32 => br.ReadUInt32(),
                _ => throw new InvalidOperationException()
            };
        }

        public static void Skip(this BinaryReader br, ValueQualifier q)
        {
            var len = q.Kind switch
            {
                ValueKind.Primitive => (uint) q.PrimitiveSize.InBytes(),
                _ => br.ReadLenPrefix(q.LenPrefixSize),
            };

            br.BaseStream.Position += len;
        }
    }
}