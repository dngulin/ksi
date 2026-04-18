namespace Ksi.Serialization
{
    public struct ValueQualifier
    {
        public ValueKind Kind;
        public LenPrefixSize LenPrefixSize;
        public PrimitiveKind PrimitiveKind;
        public PrimitiveSize PrimitiveSize;

        public ValueQualifier(ValueKind vk, LenPrefixSize lps, PrimitiveKind pk, PrimitiveSize ps)
        {
            Kind = vk;
            LenPrefixSize = lps;
            PrimitiveKind = pk;
            PrimitiveSize = ps;
        }

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

    public static class ValueQualifierExtensions
    {
        public static byte Pack(this in ValueQualifier unpacked)
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