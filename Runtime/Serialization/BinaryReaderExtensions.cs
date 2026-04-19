using System;
using System.IO;

namespace Ksi.Serialization
{
    /// <summary>
    /// Provides extension methods for <see cref="BinaryReader"/> to support Ksi serialization.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a length prefix of the specified size from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> to read from.</param>
        /// <param name="lps">The size of the length prefix.</param>
        /// <returns>The length value read from the stream.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an invalid <see cref="LenPrefixSize"/> is provided.</exception>
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

        /// <summary>
        /// Skips a value in the <see cref="BinaryReader"/> based on the provided <see cref="ValueQualifier"/>.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> to skip in.</param>
        /// <param name="q">The <see cref="ValueQualifier"/> describing the value to skip.</param>
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