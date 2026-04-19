namespace Ksi.Serialization
{
    /// <summary>
    /// Represents the size of the length prefix in bytes.
    /// </summary>
    public enum LenPrefixSize : byte
    {
        /// <summary>
        /// No length prefix (0 bytes).
        /// </summary>
        _0,

        /// <summary>
        /// 8-bit length prefix (1 byte).
        /// </summary>
        _8,

        /// <summary>
        /// 16-bit length prefix (2 bytes).
        /// </summary>
        _16,

        /// <summary>
        /// 32-bit length prefix (4 bytes).
        /// </summary>
        _32,
    }
}