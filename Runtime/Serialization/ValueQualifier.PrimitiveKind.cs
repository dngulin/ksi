namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the kind of a primitive value.
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