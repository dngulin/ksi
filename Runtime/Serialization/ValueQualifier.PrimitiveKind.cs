namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the primitive value kind.
    /// </summary>
    public enum PrimitiveKind : byte
    {
        /// <summary>
        /// A signed integer value.
        /// </summary>
        SignedInt,

        /// <summary>
        /// An unsigned integer value.
        /// </summary>
        UnsignedInt,

        /// <summary>
        /// A floating-point value.
        /// </summary>
        FloatPoint,
    }
}