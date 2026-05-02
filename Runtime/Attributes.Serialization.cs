using System;

namespace Ksi
{
    /// <summary>
    /// Marks a struct for Ksi binary serialization source generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiSerializableAttribute : Attribute
    {
    }

    /// <summary>
    /// Marks a serializable field and specifies its binary field id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class KsiSerializeFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets the binary field identifier.
        /// </summary>
        public byte Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KsiSerializeFieldAttribute"/> class.
        /// </summary>
        /// <param name="id">The binary field identifier.</param>
        public KsiSerializeFieldAttribute(byte id) => Id = id;
    }
}