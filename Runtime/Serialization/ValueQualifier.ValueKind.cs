namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the kind of value being serialized.
    /// </summary>
    public enum ValueKind : byte
    {
        /// <summary>
        /// A primitive value.
        /// </summary>
        Primitive,

        /// <summary>
        /// A repeated primitive value: <c>TRefList&lt;TPrimitive&gt;</c>.
        /// </summary>
        RepeatedPrimitive,

        /// <summary>
        /// A struct value.
        /// </summary>
        Struct,

        /// <summary>
        /// A repeated struct value: <c>TRefList&lt;TStruct&gt;</c>.
        /// </summary>
        RepeatedStruct,
    }
}