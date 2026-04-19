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
        /// A repeated primitive value (e.g., an array or list of primitives).
        /// </summary>
        RepeatedPrimitive,

        /// <summary>
        /// A struct value.
        /// </summary>
        Struct,

        /// <summary>
        /// A repeated struct value (e.g., an array or list of structs).
        /// </summary>
        RepeatedStruct,
    }
}