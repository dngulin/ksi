namespace Ksi.Serialization
{
    /// <summary>
    /// <para>
    /// Describes a value in the Ksi binary format.
    /// It is serialized as a single byte and prefixes any serialized value.
    /// </para>
    /// <para>
    /// Serialization layouts:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="ValueKind.Primitive">Primitive</see>:
    /// <c>Qualifier, Value</c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ValueKind.RepeatedPrimitive">RepeatedPrimitive</see>:
    /// <c>Qualifier, [Length, Value, Value, ...]</c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ValueKind.Struct">Struct</see>:
    /// <c>Qualifier, [Length, Value]</c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ValueKind.RepeatedStruct">RepeatedStruct</see>:
    /// <c>Qualifier, [Length, ItemCount, Qualifier, [LenPrefix, Value], Qualifier, [LenPrefix, Value], ...]</c>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public struct ValueQualifier
    {
        /// <summary>
        /// The kind of the value.
        /// </summary>
        public ValueKind Kind;

        /// <summary>
        /// The size of the length prefix for the value.
        /// Is used only for len-prefixed values:
        /// <see cref="ValueKind.RepeatedPrimitive"/>,
        /// <see cref="ValueKind.Struct"/>,
        /// <see cref="ValueKind.RepeatedStruct"/>.
        /// </summary>
        public LenPrefixSize LenPrefixSize;

        /// <summary>
        /// The kind of the primitive value.
        /// Is used only for primitive-based values:
        /// <see cref="ValueKind.Primitive"/>,
        /// <see cref="ValueKind.RepeatedPrimitive"/>.
        /// </summary>
        public PrimitiveKind PrimitiveKind;

        /// <summary>
        /// <para>
        /// The size of the primitive value.
        /// Is used only for primitive-based value values:
        /// <see cref="ValueKind.Primitive"/>,
        /// <see cref="ValueKind.RepeatedPrimitive"/>.
        /// </para>
        /// <para>
        /// For <see cref="ValueKind.RepeatedStruct"/> that field is reinterpreted as
        /// <see cref="ValueQualifierExtensions.ItemCountPrefixSize">ItemCountPrefixSize</see>.
        /// </para>
        /// </summary>
        public PrimitiveSize PrimitiveSize;

        private ValueQualifier(ValueKind vk, LenPrefixSize lps, PrimitiveKind pk, PrimitiveSize ps)
        {
            Kind = vk;
            LenPrefixSize = lps;
            PrimitiveKind = pk;
            PrimitiveSize = ps;
        }

        /// <summary>
        /// Creates a <see cref="ValueQualifier"/> for a primitive value.
        /// </summary>
        /// <param name="pk">The kind of the primitive value.</param>
        /// <param name="ps">The size of the primitive value.</param>
        /// <returns>A new <see cref="ValueQualifier"/>.</returns>
        public static ValueQualifier Primitive(PrimitiveKind pk, PrimitiveSize ps)
        {
            return new ValueQualifier(ValueKind.Primitive, default, pk, ps);
        }

        /// <summary>
        /// Creates a <see cref="ValueQualifier"/> for a repeated primitive value.
        /// </summary>
        /// <param name="pk">The kind of the primitive value.</param>
        /// <param name="ps">The size of the primitive value.</param>
        /// <param name="lps">The size of the length prefix.</param>
        /// <returns>A new <see cref="ValueQualifier"/>.</returns>
        public static ValueQualifier RepeatedPrimitive(PrimitiveKind pk, PrimitiveSize ps, LenPrefixSize lps)
        {
            return new ValueQualifier(ValueKind.RepeatedPrimitive, lps, pk, ps);
        }

        /// <summary>
        /// Creates a <see cref="ValueQualifier"/> for a struct value.
        /// </summary>
        /// <param name="lps">The size of the length prefix.</param>
        /// <returns>A new <see cref="ValueQualifier"/>.</returns>
        public static ValueQualifier Struct(LenPrefixSize lps)
        {
            return new ValueQualifier(ValueKind.Struct, lps, default, default);
        }

        /// <summary>
        /// Creates a <see cref="ValueQualifier"/> for a repeated struct value.
        /// </summary>
        /// <param name="lps">The size of the length prefix.</param>
        /// <param name="count">The number of items in the repeated struct.</param>
        /// <returns>A new <see cref="ValueQualifier"/>.</returns>
        public static ValueQualifier RepeatedStruct(LenPrefixSize lps, uint count)
        {
            var cps = GetLenPrefixSize(count);
            return new ValueQualifier(ValueKind.RepeatedStruct, lps, default, (PrimitiveSize) cps);
        }

        /// <summary>
        /// Gets the required <see cref="LenPrefixSize"/> to store the specified length.
        /// </summary>
        /// <param name="len">The length value.</param>
        /// <returns>The smallest <see cref="LenPrefixSize"/> that can accommodate the length.</returns>
        public static LenPrefixSize GetLenPrefixSize(uint len)
        {
            return len switch
            {
                0 => LenPrefixSize._0,
                <= byte.MaxValue => LenPrefixSize._8,
                <= ushort.MaxValue => LenPrefixSize._16,
                <= uint.MaxValue => LenPrefixSize._32,
            };
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

        /// <summary>
        /// Gets the <see cref="LenPrefixSize"/> for the item count in a <see cref="ValueKind.RepeatedStruct"/>.
        /// </summary>
        /// <param name="self">The <see cref="ValueQualifier"/>.</param>
        /// <returns>The <see cref="LenPrefixSize"/> used for item count.</returns>
        public static LenPrefixSize ItemCountPrefixSize(this in ValueQualifier self)
        {
            return (LenPrefixSize) self.PrimitiveSize;
        }
    }
}