namespace Ksi.Serialization
{
    /// <summary>
    /// Describes a value in the Ksi binary format.
    /// It is serialized as a single byte and prefixes any serialized value.
    /// </summary>
    public struct ValueQualifier
    {
        /// <summary>
        /// The kind of the value.
        /// </summary>
        public ValueKind Kind;

        /// <summary>
        /// The size of the length prefix for the value.
        /// Is set only for len-prefixed values:
        /// <see cref="ValueKind.RepeatedPrimitive"/>,
        /// <see cref="ValueKind.Struct"/>,
        /// <see cref="ValueKind.RepeatedStruct"/>.
        /// </summary>
        public LenPrefixSize LenPrefixSize;

        /// <summary>
        /// The kind of the primitive value.
        /// Is set only for primitive-based values:
        /// <see cref="ValueKind.Primitive"/>,
        /// <see cref="ValueKind.RepeatedPrimitive"/>.
        /// </summary>
        public PrimitiveKind PrimitiveKind;

        /// <summary>
        /// The size of the primitive value.
        /// Is set only for primitive-based value values:
        /// <see cref="ValueKind.Primitive"/>,
        /// <see cref="ValueKind.RepeatedPrimitive"/>.
        /// </summary>
        public PrimitiveSize PrimitiveSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueQualifier"/> struct.
        /// </summary>
        /// <param name="vk">The value kind.</param>
        /// <param name="lps">The length prefix size.</param>
        /// <param name="pk">The primitive kind.</param>
        /// <param name="ps">The primitive size.</param>
        public ValueQualifier(ValueKind vk, LenPrefixSize lps, PrimitiveKind pk, PrimitiveSize ps)
        {
            Kind = vk;
            LenPrefixSize = lps;
            PrimitiveKind = pk;
            PrimitiveSize = ps;
        }

        /// <summary>
        /// Unpacks a <see cref="ValueQualifier"/> from a packed <see cref="byte"/>.
        /// </summary>
        /// <param name="packed">The packed byte.</param>
        /// <returns>The unpacked <see cref="ValueQualifier"/>.</returns>
        public static ValueQualifier Unpack(byte packed)
        {
            ValueQualifier unpacked = default;

            const byte mask = 0b11;

            unpacked.PrimitiveSize = (PrimitiveSize)(packed & mask);

            packed = (byte)(packed >> 2);
            unpacked.PrimitiveKind = (PrimitiveKind)(packed & mask);

            packed = (byte)(packed >> 2);
            unpacked.LenPrefixSize = (LenPrefixSize)(packed & mask);

            packed = (byte)(packed >> 2);
            unpacked.Kind = (ValueKind)(packed & mask);

            return unpacked;
        }
    }

    /// <summary>
    /// Provides extension methods for <see cref="ValueQualifier"/>.
    /// </summary>
    public static class ValueQualifierExtensions
    {
        /// <summary>
        /// Packs a <see cref="ValueQualifier"/> into a single <see cref="byte"/>.
        /// </summary>
        /// <param name="unpacked">The <see cref="ValueQualifier"/> to pack.</param>
        /// <returns>The packed byte.</returns>
        public static byte Packed(this in ValueQualifier unpacked)
        {
            var packed = (byte)unpacked.Kind;

            packed = (byte)(packed << 2);
            packed |= (byte)unpacked.LenPrefixSize;

            packed = (byte)(packed << 2);
            packed |= (byte)unpacked.PrimitiveKind;

            packed = (byte)(packed << 2);
            packed |= (byte)unpacked.PrimitiveSize;

            return packed;
        }
    }
}