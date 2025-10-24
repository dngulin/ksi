namespace Ksi
{
    /// <summary>
    /// An enum indicating a slot state for open addressing hash tables with lazy deletion.
    /// </summary>
    public enum KsiHashTableSlotState
    {
        /// <summary>
        /// Slot doesn't store a value.
        /// </summary>
        Empty,

        /// <summary>
        /// Slot stores a value.
        /// </summary>
        Occupied,

        /// <summary>
        /// <para>
        /// Slot doesn't store a value because it was deleted.
        /// Slots with that state are treated as empty ones during insertions and as occupied ones during lookups.
        /// </para>
        /// <para>
        /// Only trailing deleted slot sequences are set to empty after item deletion.
        /// </para>
        /// </summary>
        Deleted,
    }
}