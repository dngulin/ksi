using System;
using System.IO;

namespace Ksi.Serialization
{
    /// <summary>
    /// Provides extension methods for <see cref="BinaryWriter"/> to support Ksi serialization.
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Prepends a <see cref="byte"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, byte value)
        {
            self.BaseStream.Position -= 1;
            self.Write(value);
            self.BaseStream.Position -= 1;
        }

        /// <summary>
        /// Prepends an <see cref="sbyte"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, sbyte value)
        {
            self.BaseStream.Position -= 1;
            self.Write(value);
            self.BaseStream.Position -= 1;
        }

        /// <summary>
        /// Prepends a <see cref="bool"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, bool value)
        {
            self.BaseStream.Position -= 1;
            self.Write(value);
            self.BaseStream.Position -= 1;
        }

        /// <summary>
        /// Prepends a <see cref="ushort"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, ushort value)
        {
            self.BaseStream.Position -= 2;
            self.Write(value);
            self.BaseStream.Position -= 2;
        }

        /// <summary>
        /// Prepends a <see cref="short"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, short value)
        {
            self.BaseStream.Position -= 2;
            self.Write(value);
            self.BaseStream.Position -= 2;
        }

        /// <summary>
        /// Prepends a <see cref="uint"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, uint value)
        {
            self.BaseStream.Position -= 4;
            self.Write(value);
            self.BaseStream.Position -= 4;
        }

        /// <summary>
        /// Prepends an <see cref="int"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, int value)
        {
            self.BaseStream.Position -= 4;
            self.Write(value);
            self.BaseStream.Position -= 4;
        }

        /// <summary>
        /// Prepends a <see cref="float"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, float value)
        {
            self.BaseStream.Position -= 4;
            self.Write(value);
            self.BaseStream.Position -= 4;
        }

        /// <summary>
        /// Prepends a <see cref="ulong"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, ulong value)
        {
            self.BaseStream.Position -= 8;
            self.Write(value);
            self.BaseStream.Position -= 8;
        }

        /// <summary>
        /// Prepends a <see cref="long"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, long value)
        {
            self.BaseStream.Position -= 8;
            self.Write(value);
            self.BaseStream.Position -= 8;
        }

        /// <summary>
        /// Prepends a <see cref="double"/> to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="value">The value to prepend.</param>
        public static void Prepend(this BinaryWriter self, double value)
        {
            self.BaseStream.Position -= 8;
            self.Write(value);
            self.BaseStream.Position -= 8;
        }

        /// <summary>
        /// Prepends a length prefix to the stream.
        /// </summary>
        /// <param name="self">The <see cref="BinaryWriter"/>.</param>
        /// <param name="len">The length to prepend.</param>
        /// <param name="lps">The size of the length prefix that was prepended.</param>
        public static void PrependLenPrefix(this BinaryWriter self, uint len, out LenPrefixSize lps)
        {
            lps = ValueQualifier.GetLenPrefixSize(len);
            switch (lps)
            {
                case LenPrefixSize._0:
                    break;
                case LenPrefixSize._8:
                    self.Prepend((byte)len);
                    break;
                case LenPrefixSize._16:
                    self.Prepend((ushort)len);
                    break;
                case LenPrefixSize._32:
                    self.Prepend(len);
                    break;
                default:
                    throw new Exception($"Unreachable! Invalid LenPrefixSize estimation: {(byte)lps}");
            }
        }
    }
}