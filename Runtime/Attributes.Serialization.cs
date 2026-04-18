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
        public ushort Id { get; }

        public KsiSerializeFieldAttribute(ushort id) => Id = id;
    }
}